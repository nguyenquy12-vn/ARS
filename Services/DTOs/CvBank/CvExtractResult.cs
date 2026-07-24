namespace Services.DTOs.CvBank;

// Kết quả AI trích xuất thông tin từ nội dung text của một CV.
public class CvExtractResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }

    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? CurrentTitle { get; set; }
    public double TotalYearsExperience { get; set; }
    public double AiYearsExperience { get; set; }
    public bool IsFresher { get; set; }
    public List<string> Skills { get; set; } = new();
    public string? Summary { get; set; }
    public List<string> Strengths { get; set; } = new();
    public List<string> Weaknesses { get; set; } = new();

    public static CvExtractResult Failure(string message) =>
        new() { IsSuccess = false, ErrorMessage = message };
}
