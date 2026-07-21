using Domain.Enums;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Services.DTOs.JobPosting;
using Services.Interfaces;

namespace Services.Implementations;

public class JobPostingService : IJobPostingService
{
    private readonly ARSDbContext _context;

    public JobPostingService(ARSDbContext context)
    {
        _context = context;
    }

    public async Task<List<JobPostingListDto>> GetActiveJobsAsync(string? keyword, int? categoryId, JobType? jobType, WorkMode? workMode)
    {
        var query = _context.JobPostings
            .Include(x => x.Company)
            .Include(x => x.JobCategory)
            .Where(x => x.Status == JobStatus.Active)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim().ToLower();
            query = query.Where(x => x.Title.ToLower().Contains(kw) || 
                                     (x.Company != null && x.Company.CompanyName.ToLower().Contains(kw)) || 
                                     x.Location.ToLower().Contains(kw));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(x => x.JobCategoryId == categoryId.Value);
        }

        if (jobType.HasValue)
        {
            query = query.Where(x => x.JobType == jobType.Value);
        }

        if (workMode.HasValue)
        {
            query = query.Where(x => x.WorkMode == workMode.Value);
        }

        var jobs = await query
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new JobPostingListDto
            {
                Id = x.Id,
                Title = x.Title,
                CompanyName = x.Company != null ? x.Company.CompanyName : string.Empty,
                CompanyLogoPath = x.Company != null ? x.Company.LogoPath : null,
                Location = x.Location,
                JobTypeName = x.JobType == JobType.FullTime ? "Toàn thời gian" :
                              x.JobType == JobType.PartTime ? "Bán thời gian" :
                              x.JobType == JobType.Internship ? "Thực tập" :
                              x.JobType == JobType.Contract ? "Hợp đồng" : "",
                WorkModeName = x.WorkMode == WorkMode.Onsite ? "Tại văn phòng" :
                               x.WorkMode == WorkMode.Remote ? "Từ xa" :
                               x.WorkMode == WorkMode.Hybrid ? "Kết hợp" : "",
                MinSalary = x.MinSalary,
                MaxSalary = x.MaxSalary,
                CreatedAt = x.CreatedAt,
                ExpiredAt = x.ExpiredAt,
                CategoryName = x.JobCategory != null ? x.JobCategory.Name : string.Empty
            })
            .ToListAsync();

        return jobs;
    }

    public async Task<JobPostingDetailDto?> GetJobDetailAsync(int id)
    {
        var job = await _context.JobPostings
            .Include(x => x.Company)
            .Include(x => x.JobCategory)
            .Where(x => x.Id == id && x.Status == JobStatus.Active)
            .Select(x => new JobPostingDetailDto
            {
                Id = x.Id,
                Title = x.Title,
                CompanyName = x.Company != null ? x.Company.CompanyName : string.Empty,
                CompanyLogoPath = x.Company != null ? x.Company.LogoPath : null,
                Location = x.Location,
                JobTypeName = x.JobType == JobType.FullTime ? "Toàn thời gian" :
                              x.JobType == JobType.PartTime ? "Bán thời gian" :
                              x.JobType == JobType.Internship ? "Thực tập" :
                              x.JobType == JobType.Contract ? "Hợp đồng" : "",
                WorkModeName = x.WorkMode == WorkMode.Onsite ? "Tại văn phòng" :
                               x.WorkMode == WorkMode.Remote ? "Từ xa" :
                               x.WorkMode == WorkMode.Hybrid ? "Kết hợp" : "",
                MinSalary = x.MinSalary,
                MaxSalary = x.MaxSalary,
                CreatedAt = x.CreatedAt,
                ExpiredAt = x.ExpiredAt,
                CategoryName = x.JobCategory != null ? x.JobCategory.Name : string.Empty,
                Description = x.Description,
                Requirements = x.Requirements,
                Benefits = x.Benefits,
                Vacancies = x.Vacancies,
                CompanyId = x.CompanyId,
                CompanyOverview = x.Company != null ? x.Company.Overview : null,
                CompanyAddress = x.Company != null ? x.Company.Address : null,
                CompanyWebsite = x.Company != null ? x.Company.Website : null,
                CompanySize = x.Company != null ? x.Company.CompanySize : null,
                JobType = x.JobType,
                WorkMode = x.WorkMode
            })
            .FirstOrDefaultAsync();

        return job;
    }

    public async Task<List<JobPostingListDto>> GetLatestJobsAsync(int count)
    {
        var jobs = await _context.JobPostings
            .Include(x => x.Company)
            .Include(x => x.JobCategory)
            .Where(x => x.Status == JobStatus.Active)
            .OrderByDescending(x => x.CreatedAt)
            .Take(count)
            .Select(x => new JobPostingListDto
            {
                Id = x.Id,
                Title = x.Title,
                CompanyName = x.Company != null ? x.Company.CompanyName : string.Empty,
                CompanyLogoPath = x.Company != null ? x.Company.LogoPath : null,
                Location = x.Location,
                JobTypeName = x.JobType == JobType.FullTime ? "Toàn thời gian" :
                              x.JobType == JobType.PartTime ? "Bán thời gian" :
                              x.JobType == JobType.Internship ? "Thực tập" :
                              x.JobType == JobType.Contract ? "Hợp đồng" : "",
                WorkModeName = x.WorkMode == WorkMode.Onsite ? "Tại văn phòng" :
                               x.WorkMode == WorkMode.Remote ? "Từ xa" :
                               x.WorkMode == WorkMode.Hybrid ? "Kết hợp" : "",
                MinSalary = x.MinSalary,
                MaxSalary = x.MaxSalary,
                CreatedAt = x.CreatedAt,
                ExpiredAt = x.ExpiredAt,
                CategoryName = x.JobCategory != null ? x.JobCategory.Name : string.Empty
            })
            .ToListAsync();

        return jobs;
    }
}
