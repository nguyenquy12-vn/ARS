namespace Services.DTOs.JobPosting;

public class JobPostingResult
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public int JobPostingId { get; set; }

    public static JobPostingResult Success(int id) => new() { IsSuccess = true, JobPostingId = id };

    public static JobPostingResult Failure(string message) => new() { IsSuccess = false, ErrorMessage = message };
}
