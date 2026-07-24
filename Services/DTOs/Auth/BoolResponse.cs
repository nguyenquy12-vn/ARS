namespace Services.DTOs.Auth;

public class BoolResponse
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;

    public static BoolResponse Success() => new() { IsSuccess = true };

    public static BoolResponse Failure(string message) => new() { IsSuccess = false, ErrorMessage = message };
}
