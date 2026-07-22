namespace Services.DTOs.Application;

public class JobApplicationDto
{
    public int Id { get; set; }
    public int CandidateId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public DateTime AppliedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}
