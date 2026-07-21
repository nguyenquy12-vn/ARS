using Domain.Enums;

namespace Services.DTOs.JobPosting;

public class JobPostingListItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public JobType JobType { get; set; }
    public WorkMode WorkMode { get; set; }
    public JobStatus Status { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int? MinSalary { get; set; }
    public int? MaxSalary { get; set; }
    public int Vacancies { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiredAt { get; set; }
    public int ApplicationCount { get; set; }
}
