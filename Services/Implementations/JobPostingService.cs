using Domain.Constraints;
using Domain.Entities;
using Domain.Enums;
using Infrastructure;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Services.DTOs.JobPosting;
using Services.Interfaces;

namespace Services.Implementations;

public class JobPostingService : IJobPostingService
{
    private readonly ARSDbContext _context;
    private readonly IMapper _mapper;

    public JobPostingService(ARSDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<JobPostingListItem>> GetRecruiterJobsAsync(int recruiterId)
    {
        return await _context.JobPostings
            .Where(j => j.Company!.RecruiterId == recruiterId)
            .OrderByDescending(j => j.CreatedAt)
            .Select(j => new JobPostingListItem
            {
                Id = j.Id,
                Title = j.Title,
                Location = j.Location,
                JobType = j.JobType,
                WorkMode = j.WorkMode,
                Status = j.Status,
                CategoryName = j.JobCategory != null ? j.JobCategory.Name : string.Empty,
                MinSalary = j.MinSalary,
                MaxSalary = j.MaxSalary,
                Vacancies = j.Vacancies,
                CreatedAt = j.CreatedAt,
                ExpiredAt = j.ExpiredAt,
                ApplicationCount = j.Applications.Count
            })
            .ToListAsync();
    }

    public async Task<JobPostingDetail?> GetForRecruiterAsync(int id, int recruiterId)
    {
        return await _context.JobPostings
            .Where(j => j.Id == id && j.Company!.RecruiterId == recruiterId)
            .Select(j => new JobPostingDetail
            {
                Id = j.Id,
                Title = j.Title,
                Description = j.Description,
                Requirements = j.Requirements,
                Benefits = j.Benefits,
                Location = j.Location,
                JobType = j.JobType,
                WorkMode = j.WorkMode,
                JobCategoryId = j.JobCategoryId,
                Status = j.Status,
                MinSalary = j.MinSalary,
                MaxSalary = j.MaxSalary,
                Vacancies = j.Vacancies,
                ExpiredAt = j.ExpiredAt,
                CreatedAt = j.CreatedAt,
                CategoryName = j.JobCategory != null ? j.JobCategory.Name : string.Empty,
                CompanyName = j.Company != null ? j.Company.CompanyName : string.Empty,
                ApplicationCount = j.Applications.Count,
                AiWeightExperience = j.AiWeightExperience,
                AiWeightSkills = j.AiWeightSkills,
                AiWeightEducation = j.AiWeightEducation,
                AiWeightAchievement = j.AiWeightAchievement,
                AiPriorityNote = j.AiPriorityNote
            })
            .FirstOrDefaultAsync();
    }

    public async Task<JobPostingResult> CreateAsync(int recruiterId, CreateJobPostingRequest request)
    {
        var company = await _context.Companies
            .FirstOrDefaultAsync(c => c.RecruiterId == recruiterId);

        if (company == null)
        {
            return JobPostingResult.Failure(ErrorMessage.CompanyProfileRequired);
        }

        var categoryExists = await _context.JobCategories.AnyAsync(c => c.Id == request.JobCategoryId);
        if (!categoryExists)
        {
            return JobPostingResult.Failure(ErrorMessage.JobCategoryNotFound);
        }

        var job = new JobPosting
        {
            CompanyId = company.Id,
            Title = request.Title.Trim(),
            Description = request.Description,
            Requirements = request.Requirements,
            Benefits = request.Benefits,
            Location = request.Location.Trim(),
            JobType = request.JobType,
            WorkMode = request.WorkMode,
            JobCategoryId = request.JobCategoryId,
            Status = request.Status,
            MinSalary = request.MinSalary,
            MaxSalary = request.MaxSalary,
            Vacancies = request.Vacancies,
            ExpiredAt = request.ExpiredAt,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            _context.JobPostings.Add(job);
            await _context.SaveChangesAsync();
            return JobPostingResult.Success(job.Id);
        }
        catch (Exception)
        {
            return JobPostingResult.Failure(ErrorMessage.JobSaveError);
        }
    }

    public async Task<JobPostingResult> UpdateAsync(int id, int recruiterId, UpdateJobPostingRequest request)
    {
        var job = await _context.JobPostings
            .FirstOrDefaultAsync(j => j.Id == id && j.Company!.RecruiterId == recruiterId);

        if (job == null)
        {
            return JobPostingResult.Failure(ErrorMessage.JobNotFound);
        }

        var categoryExists = await _context.JobCategories.AnyAsync(c => c.Id == request.JobCategoryId);
        if (!categoryExists)
        {
            return JobPostingResult.Failure(ErrorMessage.JobCategoryNotFound);
        }

        job.Title = request.Title.Trim();
        job.Description = request.Description;
        job.Requirements = request.Requirements;
        job.Benefits = request.Benefits;
        job.Location = request.Location.Trim();
        job.JobType = request.JobType;
        job.WorkMode = request.WorkMode;
        job.JobCategoryId = request.JobCategoryId;
        job.Status = request.Status;
        job.MinSalary = request.MinSalary;
        job.MaxSalary = request.MaxSalary;
        job.Vacancies = request.Vacancies;
        job.ExpiredAt = request.ExpiredAt;

        try
        {
            await _context.SaveChangesAsync();
            return JobPostingResult.Success(job.Id);
        }
        catch (Exception)
        {
            return JobPostingResult.Failure(ErrorMessage.JobSaveError);
        }
    }

    public async Task<JobPostingResult> DeleteAsync(int id, int recruiterId)
    {
        var job = await _context.JobPostings
            .FirstOrDefaultAsync(j => j.Id == id && j.Company!.RecruiterId == recruiterId);

        if (job == null)
        {
            return JobPostingResult.Failure(ErrorMessage.JobNotFound);
        }

        try
        {
            _context.JobPostings.Remove(job);
            await _context.SaveChangesAsync();
            return JobPostingResult.Success(id);
        }
        catch (Exception)
        {
            return JobPostingResult.Failure(ErrorMessage.JobSaveError);
        }
    }

    public async Task<List<JobCategoryOption>> GetCategoriesAsync()
    {
        return await _context.JobCategories
            .OrderBy(c => c.Name)
            .Select(c => new JobCategoryOption { Id = c.Id, Name = c.Name })
            .ToListAsync();
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
// homepage lấy việc làm mới nhất 
    public async Task<List<JobPostingListDto>> GetLatestJobsAsync(int count, int skip = 0)
    {
        var jobs = await _context.JobPostings
            .Include(x => x.Company)
            .Include(x => x.JobCategory)
            .Where(x => x.Status == JobStatus.Active)
              .OrderByDescending(x => x.CreatedAt)
            .Skip(skip)
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
//đếm tổng số việc làm
    public Task<int> CountActiveJobsAsync() =>
        _context.JobPostings.CountAsync(x => x.Status == JobStatus.Active);

    public async Task<List<JobListItem>> GetAllJobsAsync()
    {
        var jobs = await _context.JobPostings
            .Include(j => j.Company)
            .Include(j => j.JobCategory)
            .Include(j => j.Applications)
            .ToListAsync();
        return _mapper.Map<List<JobListItem>>(jobs);
    }

    public async Task<JobDetailsResponse> GetJobDetailsAsync(int id)
    {
        var job = await _context.JobPostings
            .Include(j => j.Company)
            .Include(j => j.JobCategory)
            .Include(j => j.Applications)
            .ThenInclude(a => a.Candidate)
            .FirstOrDefaultAsync(j => j.Id == id);
        if (job == null)
        {
            return JobDetailsResponse.Failure(ErrorMessage.JobNotFound);
        }
        
        return JobDetailsResponse.Success(_mapper.Map<JobDto>(job));
    }
}
