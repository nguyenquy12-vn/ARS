using Domain.Constraints;
using Mapster;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.DTOs.Auth;
using Services.Interfaces;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using WebApp.Models.Auth;
using WebApp.Accounts;

namespace WebApp.Controllers;

// [BẢO VỆ] XÁC THỰC: đăng ký, OTP, đăng nhập thường/Google, đổi-quên mật khẩu và đăng xuất.
// Luồng: form -> AuthController -> AuthService -> tạo cookie SignInAsync -> redirect theo Role.
public class AuthController : Controller
{
    private readonly IAuthService _authService;
    private readonly IAccountEmailService _accountEmailService;
    private readonly IConfiguration _configuration;

    private const string PendingRegistrationKey = "PendingRegistration";
    private const string PendingRegistrationOtpKey = "PendingRegistrationOtp";
    private const string PendingRegistrationAttemptsKey = "PendingRegistrationAttempts";
    private const string PendingResetKey = "PendingPasswordReset";
    private const string PendingResetOtpKey = "PendingPasswordResetOtp";

    public AuthController(IAuthService authService, IAccountEmailService accountEmailService, IConfiguration configuration)
    {
        _authService = authService;
        _accountEmailService = accountEmailService;
        _configuration = configuration;
    }

    [HttpGet("register")]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost("register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        // Kiểm tra các ràng buộc [Required], [Compare] xem hợp lệ chưa
        if (!ModelState.IsValid)
        {
            return View(model); // Nếu lỗi (ví dụ mật khẩu ko khớp), trả lại giao diện kèm thông báo lỗi
        }

        if (!await _authService.IsEmailAvailableAsync(model.Email))
        {
            ModelState.AddModelError(string.Empty, ErrorMessage.DuplicateEmail);
            return View(model);
        }

        var otp = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        var mailResult = await _accountEmailService.SendRegistrationOtpAsync(model.Email.Trim(), otp);
        if (!mailResult.Success)
        {
            ModelState.AddModelError(string.Empty, mailResult.Error ?? "Không thể gửi mã xác thực.");
            return View(model);
        }

        var request = model.Adapt<RegisterRequest>();
        request.Email = request.Email.Trim().ToLowerInvariant();
        HttpContext.Session.SetString(PendingRegistrationKey, JsonSerializer.Serialize(new PendingRegistration(request, DateTime.UtcNow.AddMinutes(10), DateTime.UtcNow)));
        HttpContext.Session.SetString(PendingRegistrationOtpKey, HashOtp(otp));
        HttpContext.Session.SetInt32(PendingRegistrationAttemptsKey, 0);

        return RedirectToAction(nameof(VerifyEmail));
    }

    [HttpGet("login")]
    public IActionResult Login(string? reason)
    {
        if (reason == "locked")
        {
            ViewBag.ErrorMessage = ErrorMessage.AccountLocked;
        }

        return View();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var loginDto = model.Adapt<LoginRequest>();

        // Gọi Service xử lý tạo tài khoản
        var result = await _authService.LoginAsync(loginDto);

        if (result.IsSuccess) // Tạo cookie đăng nhập và chuyển trang
        {
            await SignInAsync(result, model.RememberMe);
            return RedirectByRole(result.User!.RoleName);
        }

        ModelState.AddModelError(string.Empty, result.ErrorMessage);

        return View(model);

    }

    [HttpGet("register/verify-email")]
    public IActionResult VerifyEmail()
    {
        var pending = GetPendingRegistration();
        if (pending == null || pending.ExpiresAt <= DateTime.UtcNow)
        {
            ClearPendingRegistration();
            TempData["RegisterError"] = "Phiên xác thực đã hết hạn. Vui lòng đăng ký lại.";
            return RedirectToAction(nameof(Register));
        }

        ViewBag.Email = pending.Request.Email;
        return View(new VerifyEmailOtpViewModel());
    }

    [HttpPost("register/verify-email")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyEmail(VerifyEmailOtpViewModel model)
    {
        var pending = GetPendingRegistration();
        if (pending == null || pending.ExpiresAt <= DateTime.UtcNow)
        {
            ClearPendingRegistration();
            TempData["RegisterError"] = "Phiên xác thực đã hết hạn. Vui lòng đăng ký lại.";
            return RedirectToAction(nameof(Register));
        }

        ViewBag.Email = pending.Request.Email;
        if (!ModelState.IsValid) return View(model);

        var attempts = HttpContext.Session.GetInt32(PendingRegistrationAttemptsKey) ?? 0;
        var expectedHash = HttpContext.Session.GetString(PendingRegistrationOtpKey);
        if (attempts >= 5 || string.IsNullOrWhiteSpace(expectedHash) || !FixedTimeEquals(expectedHash, HashOtp(model.Otp)))
        {
            HttpContext.Session.SetInt32(PendingRegistrationAttemptsKey, attempts + 1);
            ModelState.AddModelError(string.Empty, attempts >= 4 ? "Bạn đã nhập sai quá nhiều lần. Hãy gửi lại mã OTP." : "Mã OTP không chính xác.");
            return View(model);
        }

        var result = await _authService.RegisterCandidateAsync(pending.Request);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Không thể tạo tài khoản.");
            return View(model);
        }

        ClearPendingRegistration();
        TempData["RegisterSuccess"] = "Xác thực email thành công. Bạn có thể đăng nhập ngay.";
        return RedirectToAction(nameof(Login));
    }

    [HttpPost("register/resend-otp")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendOtp()
    {
        var pending = GetPendingRegistration();
        if (pending == null || pending.ExpiresAt <= DateTime.UtcNow)
        {
            ClearPendingRegistration();
            TempData["RegisterError"] = "Phiên xác thực đã hết hạn. Vui lòng đăng ký lại.";
            return RedirectToAction(nameof(Register));
        }

        if (DateTime.UtcNow - pending.LastSentAt < TimeSpan.FromSeconds(60))
        {
            TempData["OtpError"] = "Vui lòng chờ 60 giây trước khi gửi lại mã.";
            return RedirectToAction(nameof(VerifyEmail));
        }

        var otp = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        var mailResult = await _accountEmailService.SendRegistrationOtpAsync(pending.Request.Email, otp);
        if (!mailResult.Success)
        {
            TempData["OtpError"] = mailResult.Error ?? "Không thể gửi lại mã OTP.";
            return RedirectToAction(nameof(VerifyEmail));
        }

        HttpContext.Session.SetString(PendingRegistrationKey, JsonSerializer.Serialize(pending with { LastSentAt = DateTime.UtcNow, ExpiresAt = DateTime.UtcNow.AddMinutes(10) }));
        HttpContext.Session.SetString(PendingRegistrationOtpKey, HashOtp(otp));
        HttpContext.Session.SetInt32(PendingRegistrationAttemptsKey, 0);
        TempData["OtpSuccess"] = "Đã gửi mã OTP mới tới email của bạn.";
        return RedirectToAction(nameof(VerifyEmail));
    }

    [HttpGet("login/google")]
    public IActionResult GoogleLogin()
    {
        if (string.IsNullOrWhiteSpace(_configuration["GoogleAuth:ClientId"]) || string.IsNullOrWhiteSpace(_configuration["GoogleAuth:ClientSecret"]))
        {
            TempData["LoginError"] = "Đăng nhập Google chưa được cấu hình. Hãy thêm GoogleAuth vào User Secrets.";
            return RedirectToAction(nameof(Login));
        }

        var properties = new AuthenticationProperties { RedirectUri = Url.Action(nameof(GoogleCallback)) };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("login/google/callback")]
    public async Task<IActionResult> GoogleCallback()
    {
        var externalResult = await HttpContext.AuthenticateAsync("External");
        if (!externalResult.Succeeded)
        {
            TempData["LoginError"] = "Không thể xác thực với Google. Vui lòng thử lại.";
            return RedirectToAction(nameof(Login));
        }

        var email = externalResult.Principal?.FindFirstValue(ClaimTypes.Email);
        var fullName = externalResult.Principal?.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
        await HttpContext.SignOutAsync("External");

        if (string.IsNullOrWhiteSpace(email))
        {
            TempData["LoginError"] = "Google chưa cung cấp địa chỉ email của tài khoản này.";
            return RedirectToAction(nameof(Login));
        }
        // kiểm tra hoặc tạo TK
        var result = await _authService.LoginWithGoogleAsync(email, fullName);

        if (!result.IsSuccess)
        {
            TempData["LoginError"] = result.ErrorMessage ?? "Không thể đăng nhập với Google.";
            return RedirectToAction(nameof(Login));
        }

        await SignInAsync(result, false);
        return RedirectByRole(result.User!.RoleName);
    }

    [Authorize]
    [HttpGet("account/change-password")]
    public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

    [Authorize]
    [HttpPost("account/change-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _authService.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword);
        if (!result.IsSuccess) { ModelState.AddModelError(string.Empty, result.ErrorMessage); return View(model); }
        TempData["PasswordSuccess"] = "Đổi mật khẩu thành công.";
        return RedirectToAction(nameof(ChangePassword));
    }

    [HttpGet("forgot-password")]
    public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

    [HttpPost("forgot-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        if (await _authService.IsEmailAvailableAsync(model.Email))
        {
            ModelState.AddModelError(string.Empty, "Không tìm thấy tài khoản với email này.");
            return View(model);
        }
        var otp = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        var sent = await _accountEmailService.SendPasswordResetOtpAsync(model.Email.Trim(), otp);
        if (!sent.Success) { ModelState.AddModelError(string.Empty, sent.Error ?? "Không gửi được OTP."); return View(model); }
        HttpContext.Session.SetString(PendingResetKey, JsonSerializer.Serialize(new PendingReset(model.Email.Trim().ToLowerInvariant(), DateTime.UtcNow.AddMinutes(10))));
        HttpContext.Session.SetString(PendingResetOtpKey, HashOtp(otp));
        return RedirectToAction(nameof(ResetPassword));
    }

    [HttpGet("reset-password")]
    public IActionResult ResetPassword()
    {
        var pending = GetPendingReset();
        if (pending == null || pending.ExpiresAt <= DateTime.UtcNow) return RedirectToAction(nameof(ForgotPassword));
        ViewBag.Email = pending.Email;
        return View(new ResetPasswordViewModel());
    }

    [HttpPost("reset-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        var pending = GetPendingReset();
        if (pending == null || pending.ExpiresAt <= DateTime.UtcNow) { ClearPendingReset(); return RedirectToAction(nameof(ForgotPassword)); }
        ViewBag.Email = pending.Email;
        if (!ModelState.IsValid) return View(model);
        var expected = HttpContext.Session.GetString(PendingResetOtpKey);
        if (string.IsNullOrWhiteSpace(expected) || !FixedTimeEquals(expected, HashOtp(model.Otp))) { ModelState.AddModelError(string.Empty, "Mã OTP không chính xác."); return View(model); }
        var result = await _authService.ResetPasswordAsync(pending.Email, model.NewPassword);
        if (!result.IsSuccess) { ModelState.AddModelError(string.Empty, result.ErrorMessage); return View(model); }
        ClearPendingReset();
        TempData["RegisterSuccess"] = "Đặt lại mật khẩu thành công. Hãy đăng nhập bằng mật khẩu mới.";
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    [ValidateAntiForgeryToken] // Chống tấn công CSRF
    public async Task<IActionResult> Logout()
    {
        // 1. Thực hiện xóa Cookie Auth trên browser của client
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // 2. Chuyển hướng người dùng về trang Đăng nhập (hoặc Trang chủ)
        return RedirectToAction("Login", "Auth");
    }

    private async Task SignInAsync(LoginResponse result, bool isPersistent)
    {
        var user = result.User!;
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.RoleName)
        };
        claims.AddRange(result.Permissions.Select(permission => new Claim("Permission", permission)));
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)),
            new AuthenticationProperties { IsPersistent = isPersistent });
    }

    private IActionResult RedirectByRole(string roleName) => roleName switch
    {
        "Recruiter" => RedirectToAction("Index", "JobPosting"),
        "Admin" => RedirectToAction("Index", "Admin"),
        _ => RedirectToAction("Index", "Home")
    };

    private PendingRegistration? GetPendingRegistration()
    {
        var value = HttpContext.Session.GetString(PendingRegistrationKey);
        return string.IsNullOrWhiteSpace(value) ? null : JsonSerializer.Deserialize<PendingRegistration>(value);
    }

    private void ClearPendingRegistration()
    {
        HttpContext.Session.Remove(PendingRegistrationKey);
        HttpContext.Session.Remove(PendingRegistrationOtpKey);
        HttpContext.Session.Remove(PendingRegistrationAttemptsKey);
    }

    private PendingReset? GetPendingReset()
    {
        var value = HttpContext.Session.GetString(PendingResetKey);
        return string.IsNullOrWhiteSpace(value) ? null : JsonSerializer.Deserialize<PendingReset>(value);
    }

    private void ClearPendingReset()
    {
        HttpContext.Session.Remove(PendingResetKey);
        HttpContext.Session.Remove(PendingResetOtpKey);
    }

    private static string HashOtp(string otp) => Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(otp)));

    private static bool FixedTimeEquals(string left, string right) =>
        CryptographicOperations.FixedTimeEquals(Convert.FromBase64String(left), Convert.FromBase64String(right));

    private sealed record PendingRegistration(RegisterRequest Request, DateTime ExpiresAt, DateTime LastSentAt);
    private sealed record PendingReset(string Email, DateTime ExpiresAt);
}
