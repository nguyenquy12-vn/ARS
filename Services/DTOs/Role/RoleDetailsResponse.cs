namespace Services.DTOs.Role;

public class RoleDetailsResponse
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public RoleDto? Role { get; set; }
    public List<PermissionDto> Permissions { get; set; } = new List<PermissionDto>();

    public static RoleDetailsResponse Success(RoleDto role, List<PermissionDto> permissions)
    {
        return new RoleDetailsResponse { IsSuccess = true, Role = role, Permissions = permissions };
    }

    public static RoleDetailsResponse Failure(string message)
    {
        return new RoleDetailsResponse { IsSuccess = false, ErrorMessage = message };
    }
}
