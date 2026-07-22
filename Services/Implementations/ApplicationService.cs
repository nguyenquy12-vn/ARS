using Domain.Entities;
using Domain.Constraints;
using Domain.Enums;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Services.DTOs.Application;
using Services.Interfaces;

namespace Services.Implementations;

public class ApplicationService : IApplicationService
{
    private readonly ARSDbContext _context;
    private readonly IAiService _aiService;

    public ApplicationService(ARSDbContext context, IAiService aiService)
    {
        _context = context;
        _aiService = aiService;
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

    public async Task<List<ApplicantListItem>> GetApplicantsForJobAsync(int jobId, int recruiterId)
    {
        return await _context.Set<Domain.Entities.Application>()
            .Where(a => a.JobPostingId == jobId && a.JobPosting!.Company!.RecruiterId == recruiterId)
            .OrderByDescending(a => a.AiMatchScore ?? -1)
            .ThenByDescending(a => a.AppliedAt)
            .Select(a => new ApplicantListItem
            {
                Id = a.Id,
                CandidateName = a.Candidate != null ? a.Candidate.FullName : string.Empty,
                CandidateEmail = a.Candidate != null ? a.Candidate.Email : string.Empty,
                ResumeTitle = a.Resume != null ? a.Resume.Title : string.Empty,
                CoverLetter = a.CoverLetter,
                AppliedAt = a.AppliedAt,
                Status = a.Status,
                AiMatchScore = a.AiMatchScore
            })
            .ToListAsync();
    }

    public async Task<ApplicationDetail?> GetDetailAsync(int applicationId, int recruiterId)
    {
        return await _context.Set<Domain.Entities.Application>()
            .Where(a => a.Id == applicationId && a.JobPosting!.Company!.RecruiterId == recruiterId)
            .Select(a => new ApplicationDetail
            {
                Id = a.Id,
                JobPostingId = a.JobPostingId,
                JobTitle = a.JobPosting != null ? a.JobPosting.Title : string.Empty,
                CandidateName = a.Candidate != null ? a.Candidate.FullName : string.Empty,
                CandidateEmail = a.Candidate != null ? a.Candidate.Email : string.Empty,
                CandidatePhone = a.Candidate != null ? a.Candidate.PhoneNumber : null,
                ResumeTitle = a.Resume != null ? a.Resume.Title : string.Empty,
                ResumeFilePath = a.Resume != null ? a.Resume.FilePath : string.Empty,
                ResumeRawText = a.Resume != null ? a.Resume.RawTextContent : null,
                CoverLetter = a.CoverLetter,
                AppliedAt = a.AppliedAt,
                Status = a.Status,
                AiMatchScore = a.AiMatchScore,
                AiFeedback = a.AiFeedback
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ApplicationResult> UpdateStatusAsync(int applicationId, int recruiterId, ApplicationStatus status)
    {
        var application = await _context.Set<Domain.Entities.Application>()
            .FirstOrDefaultAsync(a => a.Id == applicationId && a.JobPosting!.Company!.RecruiterId == recruiterId);

        if (application == null)
        {
            return ApplicationResult.Failure(ErrorMessage.ApplicationNotFound);
        }

        application.Status = status;

        try
        {
            await _context.SaveChangesAsync();
            return ApplicationResult.Success(application.Id, application.JobPostingId);
        }
        catch (Exception)
        {
            return ApplicationResult.Failure(ErrorMessage.ApplicationSaveError);
        }
    }

    public async Task<ApplicationResult> EvaluateWithAiAsync(int applicationId, int recruiterId)
    {
        var application = await _context.Set<Domain.Entities.Application>()
            .Include(a => a.JobPosting)
            .Include(a => a.Resume)
            .FirstOrDefaultAsync(a => a.Id == applicationId && a.JobPosting!.Company!.RecruiterId == recruiterId);

        if (application == null || application.JobPosting == null)
        {
            return ApplicationResult.Failure(ErrorMessage.ApplicationNotFound);
        }

        // Ưu tiên text đã trích xuất từ CV; nếu không có thì dùng thư giới thiệu
        var cvText = application.Resume?.RawTextContent;
        if (string.IsNullOrWhiteSpace(cvText))
        {
            cvText = application.CoverLetter;
        }

        if (string.IsNullOrWhiteSpace(cvText))
        {
            return ApplicationResult.Failure(ErrorMessage.CvContentMissing);
        }

        var match = await _aiService.MatchCvAsync(
            application.JobPosting.Title,
            application.JobPosting.Description,
            application.JobPosting.Requirements,
            cvText);

        if (!match.IsSuccess)
        {
            return ApplicationResult.Failure(match.ErrorMessage ?? ErrorMessage.AiEvaluationError);
        }

        application.AiMatchScore = match.MatchScore;
        application.AiFeedback = match.Feedback;

        try
        {
            await _context.SaveChangesAsync();
            return new ApplicationResult
            {
                IsSuccess = true,
                ApplicationId = application.Id,
                JobPostingId = application.JobPostingId,
                AiMatchScore = match.MatchScore,
                AiFeedback = match.Feedback
            };
        }
        catch (Exception)
        {
            return ApplicationResult.Failure(ErrorMessage.ApplicationSaveError);
        }
    }
}
