using Domain.Enums;

namespace Services.DTOs.Application;

public class ApplicantListItem
{
    public int Id { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
    public string ResumeTitle { get; set; } = string.Empty;
    public string? CoverLetter { get; set; }
    public DateTime AppliedAt { get; set; }
    public ApplicationStatus Status { get; set; }
    public int? AiMatchScore { get; set; }

    // ===== Thông tin AI trích xuất từ CV (giống Kho CV) =====
    public string? CvTitle { get; set; }
    public double? TotalYears { get; set; }
    public double? AiYears { get; set; }
    public bool? IsFresher { get; set; }
    public List<string> Skills { get; set; } = new();
    public string? Summary { get; set; }
    public List<string> Strengths { get; set; } = new();
    public List<string> Weaknesses { get; set; } = new();
    public string? ResumeFilePath { get; set; }
}
