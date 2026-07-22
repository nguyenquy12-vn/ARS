namespace Services.DTOs.Application;

public class CvMatchResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }

    // Điểm phù hợp tổng thể (thang 0-100)
    public int MatchScore { get; set; }

    // Nhận xét dạng text để hiển thị cho recruiter
    public string Feedback { get; set; } = string.Empty;

    public static CvMatchResult Success(int score, string feedback) =>
        new() { IsSuccess = true, MatchScore = score, Feedback = feedback };

    public static CvMatchResult Failure(string message) =>
        new() { IsSuccess = false, ErrorMessage = message };
}
