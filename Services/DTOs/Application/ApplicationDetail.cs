using Domain.Enums;

namespace Services.DTOs.Application;

public class ApplicationDetail
{
    public int Id { get; set; }
    public int JobPostingId { get; set; }
    public string JobTitle { get; set; } = string.Empty;

    public string CandidateName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
    public string? CandidatePhone { get; set; }

    public string ResumeTitle { get; set; } = string.Empty;
    public string ResumeFilePath { get; set; } = string.Empty;
    public string? ResumeRawText { get; set; }

    public string? CoverLetter { get; set; }
    public DateTime AppliedAt { get; set; }
    public ApplicationStatus Status { get; set; }

    public int? AiMatchScore { get; set; }
    public string? AiFeedback { get; set; }
}
