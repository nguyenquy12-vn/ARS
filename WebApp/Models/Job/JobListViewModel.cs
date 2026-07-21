using Services.DTOs.JobPosting;
using Domain.Entities;
using Domain.Enums;

namespace WebApp.Models.Job;

public class JobListViewModel
{
    public List<JobPostingListDto> Jobs { get; set; } = new();
    public List<JobCategory> Categories { get; set; } = new();
    
    // Filter parameters
    public string? Keyword { get; set; }
    public int? CategoryId { get; set; }
    public JobType? JobType { get; set; }
    public WorkMode? WorkMode { get; set; }
}
