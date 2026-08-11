using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using WebApp.Models.Admin;

namespace WebApp.Controllers.Admin;

[Route("admin/users")]
[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
public class UserManagementController : Controller
{
    private readonly IUserService _userService;
    private readonly IAuthService _authService;
    public UserManagementController(IUserService userService, IAuthService authService)
    {
        _userService = userService;
        _authService = authService;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userService.GetUserListAsync();

        return View(users);
    }

    [HttpPost("lock")]
    public async Task<IActionResult> LockAccount(int userId)
    {
        // Gọi service để khóa tài khoản
        var result = await _authService.LockAccountAsync(userId);

        if (result.IsSuccess)
        {
            return RedirectToAction("Index");
        }

        ModelState.AddModelError(string.Empty, result.ErrorMessage);
        return RedirectToAction("Index");
    }

    [HttpPost("unlock")]
    public async Task<IActionResult> UnlockAccount(int userId)
    {
        // Gọi service để mở khóa tài khoản
        var result = await _authService.UnlockAccountAsync(userId);

        if (result.IsSuccess)
        {
            return RedirectToAction("Index");
        }

        ModelState.AddModelError(string.Empty, result.ErrorMessage);
        return RedirectToAction("Index");
    }

    [HttpPost("promote")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PromoteCandidate(int userId, string targetRole)
    {
        var result = await _userService.PromoteCandidateAsync(userId, targetRole);
        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
            ? $"Đã nâng quyền tài khoản lên {targetRole}. Người dùng cần đăng nhập lại để nhận quyền mới."
            : result.ErrorMessage;
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("details/{userId}")]
    public async Task<IActionResult> Details(int userId)
    {
        var result = await _userService.GetUserByIdAsync(userId);

        if (result.IsSuccess)
        {
            UserDetailsViewModel viewModel = new UserDetailsViewModel
            {
                User = result.User,
                Resumes = result.Resumes,
                CompanyProfile = result.CompanyProfile
            };

            return View(viewModel);
        }

        ModelState.AddModelError(string.Empty, result.ErrorMessage);
        return RedirectToAction("Index");
    }
}
