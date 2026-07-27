using Services.DTOs.Role;

namespace Services.Interfaces;

public interface IRoleService
{
    Task<List<RoleDto>> GetAllRolesAsync();

    Task<RoleDetailsResponse> GetRoleDetailsAsync(int roleId);

    Task<RoleDto> CreateRoleAsync(RoleDto dto);

    Task<RoleDto> UpdateRoleAsync(RoleDto dto);

    Task<bool> DeleteRoleAsync(int roleId);

    Task<bool> UpdateRolePermissionsAsync(int roleId, List<int> permissionIds);
}
