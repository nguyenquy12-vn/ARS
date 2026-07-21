namespace Services.DTOs.Application;

public class ApplicationResult
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public int ApplicationId { get; set; }
    public int JobPostingId { get; set; }

    // Chỉ dùng cho luồng chấm điểm AI
    public int? AiMatchScore { get; set; }
    public string? AiFeedback { get; set; }

    public static ApplicationResult Success(int applicationId, int jobPostingId) =>
        new() { IsSuccess = true, ApplicationId = applicationId, JobPostingId = jobPostingId };

    public static ApplicationResult Failure(string message) =>
        new() { IsSuccess = false, ErrorMessage = message };
}
