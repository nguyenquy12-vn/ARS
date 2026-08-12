using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("admin/roles")]
public class RoleController : Controller
{
    private readonly ARSDbContext _context;
    public RoleController(ARSDbContext context) => _context = context;
    [HttpGet] public async Task<IActionResult> Index() { ViewBag.Permissions = await _context.Permissions.OrderBy(x => x.Id).ToListAsync(); ViewBag.UserCounts = await _context.Users.GroupBy(x => x.RoleId).ToDictionaryAsync(x => x.Key, x => x.Count()); return View(await _context.Roles.Include(x => x.RolePermissions).ThenInclude(x => x.Permission).OrderBy(x => x.Id).ToListAsync()); }
    [HttpPost("permissions")][ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePermissions(int roleId, int[] permissionIds)
    {
        var role = await _context.Roles.Include(x => x.RolePermissions).FirstOrDefaultAsync(x => x.Id == roleId); if (role is null) return NotFound();
        _context.RolePermissions.RemoveRange(role.RolePermissions);
        await _context.RolePermissions.AddRangeAsync(permissionIds.Distinct().Select(id => new Domain.Entities.RolePermission { RoleId = roleId, PermissionId = id }));
        await _context.SaveChangesAsync(); TempData["Success"] = $"Đã cập nhật quyền cho {role.DisplayedName}."; return RedirectToAction(nameof(Index));
    }
}
