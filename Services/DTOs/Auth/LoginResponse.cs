namespace Services.DTOs.Auth;

public class LoginResponse
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public UserAuthResponse? User { get; set; }
    public List<string> Permissions { get; set; } = new List<string>();

    public static LoginResponse Success(UserAuthResponse user, List<string> permissions) => new() { IsSuccess = true, User = user, Permissions = permissions };

    public static LoginResponse Failure(string message) => new() { IsSuccess = false, ErrorMessage = message };
}
