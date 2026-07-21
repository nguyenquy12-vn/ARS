using Domain.Enums;

namespace Services.DTOs.Auth;

public class UserResponse
{
    public int Id { get; set; } = 0;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string RoleName { get; set; } = string.Empty;

    public string Status { get; set; } = UserStatus.Active.ToString();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

}
