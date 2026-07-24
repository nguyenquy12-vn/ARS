using Domain.Enums;
using Services.DTOs.Application;

namespace Services.DTOs.JobPosting;

public class JobDto
{
    public int Id { get; set; }

    public string JobCategoryName { get; set; } = string.Empty;

    public int RecruiterId { get; set; } = 0;

    public string CompanyName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Requirements { get; set; } = string.Empty;

    public string? Benefits { get; set; }

    public string Location { get; set; } = string.Empty;

    public string JobType { get; set; } = string.Empty;     // Full-time, Part-time, Internship, Contract

    public string WorkMode { get; set; } = string.Empty;    // On-site, Remote, Hybrid

    public int? MinSalary { get; set; }

    public int? MaxSalary { get; set; }

    public int Vacancies { get; set; }

    public string Status { get; set; } = string.Empty;      // Draft, Active, Closed, Archived

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime ExpiredAt { get; set; }

    public List<JobApplicationDto> Applications { get; set; } = new();
}
