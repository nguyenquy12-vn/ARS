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

    public AuthController(IAuthService authService)
    {
        _authService = authService;
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

        // Gọi Service xử lý tạo tài khoản
        var result = await _authService.RegisterCandidateAsync(registerDto);

        if (result.IsSuccess)
        {
            return RedirectToAction("Login");
        }

        // Nếu Email đã tồn tại hoặc có lỗi từ Service, hiển thị lỗi lên giao diện
        ModelState.AddModelError(string.Empty, result.ErrorMessage);
        return View(model);
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
