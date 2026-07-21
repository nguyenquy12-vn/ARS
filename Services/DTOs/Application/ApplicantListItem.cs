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
}
