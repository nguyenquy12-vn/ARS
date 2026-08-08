using Domain.Enums;

namespace Services.DTOs.JobPosting;

public class JobPostingDetail
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Requirements { get; set; } = string.Empty;
    public string? Benefits { get; set; }
    public string Location { get; set; } = string.Empty;
    public JobType JobType { get; set; }
    public WorkMode WorkMode { get; set; }
    public int JobCategoryId { get; set; }
    public JobStatus Status { get; set; }
    public int? MinSalary { get; set; }
    public int? MaxSalary { get; set; }
    public int Vacancies { get; set; }
    public DateTime ExpiredAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public int ApplicationCount { get; set; }
}
