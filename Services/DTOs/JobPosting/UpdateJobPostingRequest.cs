using Domain.Enums;

namespace Services.DTOs.JobPosting;

public class UpdateJobPostingRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Requirements { get; set; } = string.Empty;
    public string? Benefits { get; set; }
    public string Location { get; set; } = string.Empty;
    public JobType JobType { get; set; } = JobType.FullTime;
    public WorkMode WorkMode { get; set; } = WorkMode.Onsite;
    public int JobCategoryId { get; set; }
    public JobStatus Status { get; set; } = JobStatus.Draft;
    public int? MinSalary { get; set; }
    public int? MaxSalary { get; set; }
    public int Vacancies { get; set; } = 1;
    public DateTime ExpiredAt { get; set; }
}
