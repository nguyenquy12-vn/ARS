using Domain.Enums;

namespace Services.DTOs.JobPosting;

public class JobPostingDetailDto : JobPostingListDto
{
    public string Description { get; set; } = string.Empty;
    public string Requirements { get; set; } = string.Empty;
    public string? Benefits { get; set; }
    public int Vacancies { get; set; }
    public int CompanyId { get; set; }
    public string? CompanyOverview { get; set; }
    public string? CompanyAddress { get; set; }
    public string? CompanyWebsite { get; set; }
    public string? CompanySize { get; set; }
    public JobType JobType { get; set; }
    public WorkMode WorkMode { get; set; }
}
