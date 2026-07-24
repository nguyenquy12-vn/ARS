namespace Services.DTOs.Auth;

public class UserAuthResponse
{
    public int Id { get; set; } = 0;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string RoleName { get; set; } = string.Empty;

}
