namespace Services.DTOs.User;

public class UserDto
{
    public int Id { get; set; } = 0;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string RoleName { get; set; } = string.Empty;

    public string DisplayedRoleName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

}
