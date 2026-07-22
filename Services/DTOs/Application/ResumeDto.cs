namespace Services.DTOs.Application;

public class ResumeDto
{
    public int Id { get; set; }

    public int CandidateId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty; 

    public bool IsDefault { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public List<CandidateApplicationDto> Applications { get; set; } = new List<CandidateApplicationDto>();
}
