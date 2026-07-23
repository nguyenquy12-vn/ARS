namespace Services.DTOs.CvBank;

// Một dòng CV hiển thị trong bảng Kho CV.
public class CvBankItemDto
{
    public int Id { get; set; }
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
    public string FileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int? FolderId { get; set; }

    // Kết quả chấm điểm theo JD của thư mục
    public int? MatchScore { get; set; }
    public string? MatchVerdict { get; set; }
    public List<string> MatchedSkills { get; set; } = new();
    public List<string> MissingSkills { get; set; } = new();
    public List<string> MatchStrengths { get; set; } = new();
    public List<string> MatchConcerns { get; set; } = new();
    public bool HasMatch { get; set; }
}
