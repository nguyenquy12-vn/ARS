using Services.DTOs.Auth;

namespace Services.DTOs.JobPosting;

public class JobDetailsResponse
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;

    public JobDto? Job { get; set; }

    public static JobDetailsResponse Success(JobDto job) => new() { IsSuccess = true, Job = job };

    public static JobDetailsResponse Failure(string message) => new() { IsSuccess = false, ErrorMessage = message };
}
