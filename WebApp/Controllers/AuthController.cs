using Domain.Constraints;
using Mapster;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Services.DTOs.Auth;
using Services.Interfaces;
using System.Security.Claims;
using WebApp.Models.Auth;

namespace WebApp.Controllers;

public class AuthController : Controller
{
    private readonly IAuthService _authService;
    private readonly Microsoft.Extensions.Logging.ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, Microsoft.Extensions.Logging.ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpGet("register")]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        // Kiểm tra các ràng buộc [Required], [Compare] xem hợp lệ chưa
        if (!ModelState.IsValid)
        {
            return View(model); // Nếu lỗi (ví dụ mật khẩu ko khớp), trả lại giao diện kèm thông báo lỗi
        }

        // Nếu hợp lệ, dùng Mapster biến đổi từ ViewModel sang Request DTO của tầng Service
        var registerDto = model.Adapt<RegisterRequest>();

        try
        {
            // Gọi Service xử lý tạo tài khoản
            var result = await _authService.RegisterCandidateAsync(registerDto);

            if (result.IsSuccess)
            {
                // After registering, send OTP and ask user to verify via email
                if (!string.IsNullOrEmpty(result.ErrorMessage))
                {
                    TempData["ErrorMessage"] = result.ErrorMessage;
                }
                return RedirectToAction("VerifyEmail", new { email = model.Email });
            }

            // Nếu Email đã tồn tại hoặc có lỗi từ Service, hiển thị lỗi lên giao diện
            ModelState.AddModelError(string.Empty, result.ErrorMessage);
            return View(model);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during registration for email {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi đăng ký. Vui lòng thử lại sau.");
            return View(model);
        }
    }

    [HttpGet("external-login")]
    public IActionResult ExternalLogin(string provider = "Google")
    {
        // Ensure the external provider is registered before challenging
        var schemeProvider = HttpContext.RequestServices.GetService(typeof(Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider)) as Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider;
        var scheme = schemeProvider?.GetSchemeAsync(provider).GetAwaiter().GetResult();
        if (scheme == null)
        {
            TempData["ErrorMessage"] = "External provider not configured.";
            return RedirectToAction("Login");
        }

        var properties = new AuthenticationProperties { RedirectUri = "/external-login-callback" };
        return Challenge(properties, provider);
    }

    [HttpGet("external-login-callback")]
    public async Task<IActionResult> ExternalLoginCallback()
    {
        // Read external principal from the temporary External cookie
        var result = await HttpContext.AuthenticateAsync("External");
        var principal = result?.Principal;
        if (principal == null)
        {
            TempData["ErrorMessage"] = "External login failed.";
            return RedirectToAction("Login");
        }

        var provider = "Google";
        var providerKey = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                          ?? principal.FindFirst("sub")?.Value;
        var email = principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var name = principal.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

        var user = await _authService.GetOrCreateExternalUserAsync(provider, providerKey ?? string.Empty, email, name);
        if (user == null)
        {
            await HttpContext.SignOutAsync("External");
            return RedirectToAction("Login");
        }

        var claims = new List<System.Security.Claims.Claim>
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString()),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.FullName),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, user.Email),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, user.RoleName)
        };

        var claimsIdentity = new System.Security.Claims.ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new System.Security.Claims.ClaimsPrincipal(claimsIdentity));
        // Clear temporary external cookie
        await HttpContext.SignOutAsync("External");
        return RedirectToAction("Index", "Home");
    }

    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail(string email)
    {
        ViewBag.Email = email;
        try
        {
            var seconds = await _authService.GetResendCooldownSecondsAsync(email);
            ViewBag.ResendCooldownSeconds = seconds;
        }
        catch
        {
            ViewBag.ResendCooldownSeconds = 0;
        }

        return View();
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmailPost(string email, string code)
    {
        var result = await _authService.VerifyEmailOtpAsync(email, code);
        if (result.IsSuccess)
        {
            return RedirectToAction("Login");
        }

        ModelState.AddModelError(string.Empty, result.ErrorMessage);
        ViewBag.Email = email;
        return View("VerifyEmail");
    }

    [HttpPost("resend-otp")]
    public async Task<IActionResult> ResendOtp(string email)
    {
        var result = await _authService.ResendEmailOtpAsync(email);
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "Mã OTP mới đã được gửi đến email của bạn.";
        }
        else
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
        }

        return RedirectToAction("VerifyEmail", new { email });
    }

    [HttpGet("login")]
    public IActionResult Login(string? reason)
    {
        if (reason == "locked")
        {
            ViewBag.ErrorMessage = ErrorMessage.AccountLocked;
        }
        else if (reason == "external_failed")
        {
            ViewBag.ErrorMessage = "External authentication failed. Please check Google configuration (ClientId/ClientSecret) or try again later.";
        }

        // Preserve any TempData messages set by other flows
        if (TempData["ErrorMessage"] != null && string.IsNullOrEmpty(ViewBag.ErrorMessage))
        {
            ViewBag.ErrorMessage = TempData["ErrorMessage"] as string;
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

        if (result.IsSuccess)
        {
            var user = result.User;
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.RoleName)
            };

            foreach (var permission in result.Permissions)
            {
                claims.Add(new Claim("Permission", permission));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            switch (user.RoleName)
            {
                case "Recruiter":
                    return RedirectToAction("Index", "JobPosting");
                case "Admin":
                    return RedirectToAction("Index", "Admin");
                default:
                    return RedirectToAction("Index", "Home");
            }
        }

        ModelState.AddModelError(string.Empty, result.ErrorMessage);

        return View(model);

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
}
