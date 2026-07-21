namespace Services.DTOs.Auth;

public class RegisterResponse
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;

    public static RegisterResponse Success() => new() { IsSuccess = true };

    public static RegisterResponse Failure(string message) => new() { IsSuccess = false, ErrorMessage = message };
}
