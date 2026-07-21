using Domain.Constraints;
using Domain.Entities;
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
                ApplicationCount = j.Applications.Count
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
}
