using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using WebApp.Models.Admin;
using System.Security.Claims;

namespace WebApp.Controllers.Admin;

[Route("admin/users")]
[Microsoft.AspNetCore.Authorization.Authorize(Policy = "CanManageUsers")]
public class UserManagementController : Controller
{
    private readonly IUserService _userService;
    private readonly IAuthService _authService;
    private readonly Services.Interfaces.IAuditService _auditService;
    private readonly Microsoft.Extensions.Logging.ILogger<UserManagementController> _logger;

    public UserManagementController(IUserService userService, IAuthService authService, Services.Interfaces.IAuditService auditService, Microsoft.Extensions.Logging.ILogger<UserManagementController> logger)
    {
        _userService = userService;
        _authService = authService;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(string? search, string? role, string? status, int page = 1, int pageSize = 20)
    {
        // Diagnostic log: who is accessing UserManagement.Index and which claims they have
        try
        {
            var claims = User?.Claims?.Select(c => $"{c.Type}={c.Value}").ToList() ?? new List<string>();
            _logger?.LogInformation("UserManagement.Index accessed by {User} (Authenticated={Authenticated}). Claims: {Claims}", User?.Identity?.Name, User?.Identity?.IsAuthenticated, string.Join(";", claims));
        }
        catch { }

        var (users, total) = await _userService.GetUserListAsync(search, role, status, page, pageSize);

        var vm = new UserListViewModel
        {
            Users = users,
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
            Search = search,
            RoleFilter = role,
            StatusFilter = status
        };

        return View(vm);
    }

    [HttpPost("bulk")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkAction(string action, int[] userIds)
    {
        if (userIds == null || userIds.Length == 0)
        {
            return BadRequest("No users selected");
        }

        var currentUserId = GetCurrentUserId();

        if (action == "lock")
        {
            foreach (var id in userIds)
            {
                if (id == currentUserId || await IsAdminUserAsync(id))
                {
                    continue;
                }

                await _authService.LockAccountAsync(id);
            }
        }
        else if (action == "unlock")
        {
            foreach (var id in userIds)
            {
                await _authService.UnlockAccountAsync(id);
            }
        }
        else if (action == "export")
        {
            // redirect to export endpoint with ids
            var qs = string.Join("&", userIds.Select(i => $"ids={i}"));
            return Redirect($"/admin/users/export?{qs}");
        }

        return RedirectToAction("Index");
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export(int[] ids)
    {
        var list = new List<Services.DTOs.User.UserDto>();
        foreach (var id in ids)
        {
            var r = await _userService.GetUserByIdAsync(id);
            if (r.IsSuccess && r.User != null)
            {
                list.Add(r.User);
            }
        }

        var csv = new System.Text.StringBuilder();
        csv.AppendLine("Id,FullName,Email,Phone,Role,Status,CreatedAt");
        foreach (var u in list)
        {
            csv.AppendLine($"{u.Id},\"{u.FullName}\",{u.Email},{u.PhoneNumber},{u.RoleName},{u.Status},{u.CreatedAt:O}");
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
        return File(bytes, "text/csv", "users_export.csv");
    }

    [HttpPost("lock")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LockAccount(int userId)
    {
        if (userId == GetCurrentUserId())
        {
            TempData["ErrorMessage"] = "Không thể khóa chính tài khoản đang đăng nhập.";
            return RedirectToAction("Index");
        }

        if (await IsAdminUserAsync(userId))
        {
            TempData["ErrorMessage"] = "Không thể khóa tài khoản quản trị viên.";
            return RedirectToAction("Index");
        }

        // Gọi service để khóa tài khoản
        var result = await _authService.LockAccountAsync(userId);

        if (result.IsSuccess)
        {
            // Audit
            var actorEmail = User?.Identity?.Name;
            int? actorId = null;
            await _auditService.LogAsync(actorId, actorEmail, "LockAccount", "User", userId, $"Locked user {userId}");
            return RedirectToAction("Index");
        }

        ModelState.AddModelError(string.Empty, result.ErrorMessage);
        return RedirectToAction("Index");
    }

    [HttpPost("unlock")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnlockAccount(int userId)
    {
        // Gọi service để mở khóa tài khoản
        var result = await _authService.UnlockAccountAsync(userId);

        if (result.IsSuccess)
        {
            await _auditService.LogAsync(null, User?.Identity?.Name, "UnlockAccount", "User", userId, $"Unlocked user {userId}");
            return RedirectToAction("Index");
        }

        ModelState.AddModelError(string.Empty, result.ErrorMessage);
        return RedirectToAction("Index");
    }

    [HttpGet("details/{userId}")]
    public async Task<IActionResult> Details(int userId)
    {
        try
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
        catch (System.Exception ex)
        {
            // Log the exception and redirect to Index without setting TempData to avoid leaking the message to login page
            _logger.LogError(ex, "Error loading user details for id {UserId}. Redirecting back to index.", userId);
            return RedirectToAction("Index");
        }
    }

    private int? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id) ? id : null;
    }

    private async Task<bool> IsAdminUserAsync(int userId)
    {
        var result = await _userService.GetUserByIdAsync(userId);
        return result.IsSuccess
            && result.User != null
            && string.Equals(result.User.RoleName, "Admin", StringComparison.OrdinalIgnoreCase);
    }
}
