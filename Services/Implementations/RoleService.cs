using Domain.Entities;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Services.DTOs.Role;
using Services.Interfaces;

namespace Services.Implementations;

public class RoleService : IRoleService
{
    private readonly ARSDbContext _context;

    public RoleService(ARSDbContext context)
    {
        _context = context;
    }

    public async Task<List<RoleDto>> GetAllRolesAsync()
    {
        var roles = await _context.Roles.OrderBy(r => r.Id).ToListAsync();
        return roles.Select(r => new RoleDto { Id = r.Id, Name = r.Name, DisplayedName = r.DisplayedName, Description = r.Description }).ToList();
    }

    public async Task<RoleDetailsResponse> GetRoleDetailsAsync(int roleId)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId);
        if (role == null) return RoleDetailsResponse.Failure("Role not found");

        var perms = await _context.Permissions.OrderBy(p => p.Id).ToListAsync();
        var assigned = await _context.RolePermissions.Where(rp => rp.RoleId == roleId).Select(rp => rp.PermissionId).ToListAsync();

        var permDtos = perms.Select(p => new PermissionDto { Id = p.Id, Name = p.Name, Description = p.Description, Assigned = assigned.Contains(p.Id) }).ToList();

        var dto = new RoleDto { Id = role.Id, Name = role.Name, DisplayedName = role.DisplayedName, Description = role.Description };

        return RoleDetailsResponse.Success(dto, permDtos);
    }

    public async Task<RoleDto> CreateRoleAsync(RoleDto dto)
    {
        var role = new Role { Name = dto.Name, DisplayedName = dto.DisplayedName, Description = dto.Description };
        _context.Roles.Add(role);
        await _context.SaveChangesAsync();
        dto.Id = role.Id;
        return dto;
    }

    public async Task<RoleDto> UpdateRoleAsync(RoleDto dto)
    {
        var role = await _context.Roles.FindAsync(dto.Id);
        if (role == null) return dto;
        role.Name = dto.Name;
        role.DisplayedName = dto.DisplayedName;
        role.Description = dto.Description;
        _context.Roles.Update(role);
        await _context.SaveChangesAsync();
        return dto;
    }

    public async Task<bool> DeleteRoleAsync(int roleId)
    {
        var role = await _context.Roles.FindAsync(roleId);
        if (role == null) return false;
        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateRolePermissionsAsync(int roleId, List<int> permissionIds)
    {
        var role = await _context.Roles.FindAsync(roleId);
        if (role == null) return false;

        // Remove existing
        var existing = _context.RolePermissions.Where(rp => rp.RoleId == roleId);
        _context.RolePermissions.RemoveRange(existing);

        // Add new
        foreach (var pid in permissionIds.Distinct())
        {
            _context.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = pid });
        }

        await _context.SaveChangesAsync();
        return true;
    }
}
