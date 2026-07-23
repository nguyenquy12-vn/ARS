namespace Services.DTOs.Application;

public class CvMatchResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }

    // Điểm phù hợp tổng thể (thang 0-100)
    public int MatchScore { get; set; }

    // Kết luận ngắn: Rất phù hợp | Phù hợp | Cân nhắc | Chưa phù hợp
    public string? Verdict { get; set; }

    // Cấu trúc so sánh với JD
    public List<string> MatchedSkills { get; set; } = new();
    public List<string> MissingSkills { get; set; } = new();
    public List<string> Strengths { get; set; } = new();
    public List<string> Concerns { get; set; } = new();

    // Tóm tắt + đề xuất hành động
    public string Summary { get; set; } = string.Empty;
    public string? Recommendation { get; set; }

    // Nhận xét dạng text để hiển thị nhanh (gộp)
    public string Feedback { get; set; } = string.Empty;

    public static CvMatchResult Success(int score, string feedback) =>
        new() { IsSuccess = true, MatchScore = score, Feedback = feedback, Summary = feedback };

    public static CvMatchResult Failure(string message) =>
        new() { IsSuccess = false, ErrorMessage = message };
}
