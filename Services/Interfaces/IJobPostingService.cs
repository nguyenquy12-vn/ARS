using Domain.Enums;
using Services.DTOs.JobPosting;

namespace Services.Interfaces;

public interface IJobPostingService
{
    Task<List<JobPostingListDto>> GetActiveJobsAsync(string? keyword, int? categoryId, JobType? jobType, WorkMode? workMode);
    Task<JobPostingDetailDto?> GetJobDetailAsync(int id);
    Task<List<JobPostingListDto>> GetLatestJobsAsync(int count);
}
