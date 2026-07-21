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
}
