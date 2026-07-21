using Domain.Entities;
using Domain.Enums;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Services.DTOs.Application;
using Services.Interfaces;

namespace Services.Implementations;

public class ApplicationService : IApplicationService
{
    private readonly ARSDbContext _context;

    public ApplicationService(ARSDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ApplyJobAsync(int candidateId, int jobId, string cvFilePath, string cvFileName, string? coverLetter)
    {
        // 1. Kiểm tra việc làm có tồn tại và còn hạn không
        var job = await _context.JobPostings.FindAsync(jobId);
        if (job == null || job.Status != JobStatus.Active || job.ExpiredAt < DateTime.UtcNow)
        {
            return false;
        }

        // 2. Kiểm tra xem ứng viên đã nộp đơn cho job này chưa
        var alreadyApplied = await _context.Set<Application>().AnyAsync(a => a.CandidateId == candidateId && a.JobPostingId == jobId);
        if (alreadyApplied)
        {
            return false;
        }

        // 3. Tạo Resume (Hồ sơ)
        var resume = new Resume
        {
            CandidateId = candidateId,
            Title = cvFileName,
            FilePath = cvFilePath,
            IsDefault = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.Resumes.Add(resume);
        await _context.SaveChangesAsync(); // Lưu để lấy ResumeId

        // 4. Tạo Application (Đơn ứng tuyển)
        var application = new Application
        {
            JobPostingId = jobId,
            CandidateId = candidateId,
            ResumeId = resume.Id,
            CoverLetter = coverLetter,
            AppliedAt = DateTime.UtcNow,
            Status = ApplicationStatus.Pending
        };
        _context.Set<Application>().Add(application);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<ApplicationDto>> GetMyApplicationsAsync(int candidateId)
    {
        var applications = await _context.Set<Application>()
            .Include(a => a.JobPosting)
            .ThenInclude(j => j.Company)
            .Where(a => a.CandidateId == candidateId)
            .OrderByDescending(a => a.AppliedAt)
            .Select(a => new ApplicationDto
            {
                Id = a.Id,
                JobPostingId = a.JobPostingId,
                JobTitle = a.JobPosting.Title,
                CompanyName = a.JobPosting.Company.CompanyName,
                CompanyLogoPath = a.JobPosting.Company.LogoPath,
                AppliedAt = a.AppliedAt,
                Status = a.Status.ToString(),
                CoverLetter = a.CoverLetter,
                CancelReason = a.CancelReason,
                AiMatchScore = a.AiMatchScore
            })
            .ToListAsync();

        return applications;
    }

    public async Task<bool> WithdrawApplicationAsync(int candidateId, int applicationId, string reason)
    {
        var application = await _context.Set<Application>().FirstOrDefaultAsync(a => a.Id == applicationId && a.CandidateId == candidateId);
        if (application == null)
        {
            return false;
        }

        // Không cho phép rút nếu đã bị từ chối hoặc đã rút rồi
        if (application.Status == ApplicationStatus.Rejected || application.Status == ApplicationStatus.Withdrawn)
        {
            return false;
        }

        application.Status = ApplicationStatus.Withdrawn;
        application.CancelReason = reason;

        await _context.SaveChangesAsync();
        return true;
    }
}
