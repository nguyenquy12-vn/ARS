using Microsoft.AspNetCore.Mvc;
using Services.DTOs.Role;
using Services.Interfaces;

namespace WebApp.Controllers.Admin;

[Route("admin/roles")]
[Microsoft.AspNetCore.Authorization.Authorize(Policy = "CanManageRoles")]
public class RoleManagementController : Controller
{
    private readonly IRoleService _roleService;
    private readonly Services.Interfaces.IAuditService _auditService;

    public RoleManagementController(IRoleService roleService, Services.Interfaces.IAuditService auditService)
    {
        _roleService = roleService;
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var roles = await _roleService.GetAllRolesAsync();
        return View(roles);
    }

    [HttpGet("create")]
    public IActionResult Create()
    {
        return View(new RoleDto());
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create(RoleDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        var created = await _roleService.CreateRoleAsync(dto);
        return RedirectToAction("Index");
    }

    [HttpGet("edit/{roleId}")]
    public async Task<IActionResult> Edit(int roleId)
    {
        var details = await _roleService.GetRoleDetailsAsync(roleId);
        if (!details.IsSuccess) return RedirectToAction("Index");
        return View(details);
    }

    [HttpPost("edit/{roleId}")]
    public async Task<IActionResult> Edit(int roleId, RoleDto dto, int[] permissions)
    {
        if (!ModelState.IsValid) return View(await _roleService.GetRoleDetailsAsync(roleId));

        dto.Id = roleId;
        await _roleService.UpdateRoleAsync(dto);
        await _roleService.UpdateRolePermissionsAsync(roleId, permissions?.ToList() ?? new List<int>());
        await _auditService.LogAsync(null, User?.Identity?.Name, "UpdateRole", "Role", roleId, $"Updated role {dto.Name} (id={roleId}); permissions: {string.Join(',', permissions ?? Array.Empty<int>())}");

        return RedirectToAction("Index");
    }

    [HttpPost("delete/{roleId}")]
    public async Task<IActionResult> Delete(int roleId)
    {
        await _roleService.DeleteRoleAsync(roleId);
        await _auditService.LogAsync(null, User?.Identity?.Name, "DeleteRole", "Role", roleId, $"Deleted role id={roleId}");
        return RedirectToAction("Index");
    }
}
