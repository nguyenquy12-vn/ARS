using Domain.Enums;
using Services.DTOs.JobPosting;

namespace Services.Interfaces;

public interface IJobPostingService
{
    Task<List<JobPostingListItem>> GetRecruiterJobsAsync(int recruiterId);

    Task<JobPostingDetail?> GetForRecruiterAsync(int id, int recruiterId);

    Task<JobPostingResult> CreateAsync(int recruiterId, CreateJobPostingRequest request);

    Task<JobPostingResult> UpdateAsync(int id, int recruiterId, UpdateJobPostingRequest request);

    Task<JobPostingResult> DeleteAsync(int id, int recruiterId);

    Task<List<JobCategoryOption>> GetCategoriesAsync();
    Task<List<JobPostingListDto>> GetActiveJobsAsync(string? keyword, int? categoryId, JobType? jobType, WorkMode? workMode);
    Task<JobPostingDetailDto?> GetJobDetailAsync(int id);
    Task<List<JobPostingListDto>> GetLatestJobsAsync(int count, int skip = 0);
    Task<int> CountActiveJobsAsync();

    Task<List<JobListItem>> GetAllJobsAsync();

    Task<JobDetailsResponse> GetJobDetailsAsync(int id);
}
