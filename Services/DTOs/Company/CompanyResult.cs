namespace Services.DTOs.Company;

public class CompanyResult
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public int CompanyId { get; set; }

    public static CompanyResult Success(int id) => new() { IsSuccess = true, CompanyId = id };

    public static CompanyResult Failure(string message) => new() { IsSuccess = false, ErrorMessage = message };
}
