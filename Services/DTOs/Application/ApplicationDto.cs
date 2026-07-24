namespace Services.DTOs.Application;

public class ApplicationDto
{
    public int Id { get; set; }
    public int JobPostingId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? CompanyLogoPath { get; set; }
    public DateTime AppliedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? CoverLetter { get; set; }
    public string? CancelReason { get; set; }
    public int? AiMatchScore { get; set; }
}
