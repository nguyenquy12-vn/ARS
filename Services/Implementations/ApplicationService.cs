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

    public async Task<List<ApplicantListItem>> GetApplicantsForJobAsync(int jobId, int recruiterId)
    {
        // Chỉ đọc dữ liệu (kể cả kết quả AI đã cache). KHÔNG gọi AI ở đây để trang không bị treo
        // khi server AI không phản hồi. Việc phân tích do recruiter bấm nút (AnalyzeApplicantsAsync).
        var applications = await _context.Set<Domain.Entities.Application>()
            .Include(a => a.Candidate)
            .Include(a => a.Resume)
            .Where(a => a.JobPostingId == jobId && a.JobPosting!.Company!.RecruiterId == recruiterId)
            .OrderByDescending(a => a.AiMatchScore ?? -1)
            .ThenByDescending(a => a.AppliedAt)
            .ToListAsync();

        return applications.Select(a => new ApplicantListItem
        {
            Id = a.Id,
            CandidateName = a.Candidate != null ? a.Candidate.FullName : string.Empty,
            CandidateEmail = a.Candidate != null ? a.Candidate.Email : string.Empty,
            ResumeTitle = a.Resume != null ? a.Resume.Title : string.Empty,
            CoverLetter = a.CoverLetter,
            AppliedAt = a.AppliedAt,
            Status = a.Status,
            AiMatchScore = a.AiMatchScore,
            Verdict = a.AiVerdict,
            Recommendation = a.AiRecommendation,
            MatchSummary = a.AiFeedback,
            MatchedSkills = SplitLines(a.AiMatchedSkills),
            MissingSkills = SplitLines(a.AiMissingSkills),
            MatchStrengths = SplitLines(a.AiStrengths),
            MatchConcerns = SplitLines(a.AiConcerns),
            HasScore = a.AiScoredAt != null,
            CvTitle = a.Resume?.AiTitle,
            TotalYears = a.Resume?.AiTotalYears,
            AiYears = a.Resume?.AiAiYears,
            IsFresher = a.Resume?.AiIsFresher,
            Skills = !string.IsNullOrWhiteSpace(a.Resume?.AiSkills)
                ? a.Resume!.AiSkills!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
                : new List<string>(),
            Summary = a.Resume?.AiSummary,
            Strengths = !string.IsNullOrWhiteSpace(a.Resume?.AiStrengths)
                ? a.Resume!.AiStrengths!.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
                : new List<string>(),
            Weaknesses = !string.IsNullOrWhiteSpace(a.Resume?.AiWeaknesses)
                ? a.Resume!.AiWeaknesses!.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
                : new List<string>(),
            ResumeFilePath = a.Resume?.FilePath
        }).ToList();
    }

    public async Task<int> AnalyzeApplicantsAsync(int jobId, int recruiterId)
    {
        var resumes = await _context.Set<Domain.Entities.Application>()
            .Where(a => a.JobPostingId == jobId && a.JobPosting!.Company!.RecruiterId == recruiterId)
            .Select(a => a.Resume!)
            .Where(r => r != null && r.AiAnalyzedAt == null && r.RawTextContent != null)
            .Distinct()
            .ToListAsync();

        var analyzed = 0;
        foreach (var resume in resumes)
        {
            var extracted = await _aiService.ExtractCvInfoAsync(resume.RawTextContent!);
            if (!extracted.IsSuccess) continue;

            ApplyExtractToResume(resume, extracted);
            analyzed++;
        }

        if (analyzed > 0)
        {
            await _context.SaveChangesAsync();
        }

        return analyzed;
    }

    private static void ApplyExtractToResume(Domain.Entities.Resume resume, Services.DTOs.CvBank.CvExtractResult extracted)
    {
        resume.AiName = extracted.Name;
        resume.AiTitle = extracted.CurrentTitle;
        resume.AiTotalYears = extracted.TotalYearsExperience;
        resume.AiAiYears = extracted.AiYearsExperience;
        resume.AiIsFresher = extracted.IsFresher;
        resume.AiSkills = extracted.Skills.Count > 0 ? string.Join(", ", extracted.Skills) : null;
        resume.AiSummary = extracted.Summary;
        resume.AiStrengths = extracted.Strengths.Count > 0 ? string.Join("\n", extracted.Strengths) : null;
        resume.AiWeaknesses = extracted.Weaknesses.Count > 0 ? string.Join("\n", extracted.Weaknesses) : null;
        resume.AiAnalyzedAt = DateTime.UtcNow;
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
            cvText,
            BuildSettings(application.JobPosting));

        if (!match.IsSuccess)
        {
            return ApplicationResult.Failure(match.ErrorMessage ?? ErrorMessage.AiEvaluationError);
        }

        ApplyMatchResult(application, match);

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

    public async Task<(int scored, string? error)> ScoreApplicantsAsync(int jobId, int recruiterId, bool rescoreAll)
    {
        var job = await _context.JobPostings
            .FirstOrDefaultAsync(j => j.Id == jobId && j.Company!.RecruiterId == recruiterId);
        if (job == null)
        {
            return (0, ErrorMessage.JobNotFound);
        }

        var applications = await _context.Set<Domain.Entities.Application>()
            .Include(a => a.Resume)
            .Where(a => a.JobPostingId == jobId)
            .ToListAsync();

        var settings = BuildSettings(job);
        var scored = 0;
        string? lastError = null;

        foreach (var app in applications)
        {
            if (!rescoreAll && app.AiScoredAt != null) continue;

            var cvText = app.Resume?.RawTextContent;
            if (string.IsNullOrWhiteSpace(cvText)) cvText = app.CoverLetter;
            if (string.IsNullOrWhiteSpace(cvText)) continue;

            // Trích xuất + chấm điểm chạy song song để nhanh hơn
            var needExtract = app.Resume != null && app.Resume.AiAnalyzedAt == null
                && !string.IsNullOrWhiteSpace(app.Resume.RawTextContent);
            var extractTask = needExtract ? _aiService.ExtractCvInfoAsync(app.Resume!.RawTextContent!) : null;
            var matchTask = _aiService.MatchCvAsync(job.Title, job.Description, job.Requirements, cvText, settings);

            if (extractTask != null)
            {
                var extracted = await extractTask;
                if (extracted.IsSuccess) ApplyExtractToResume(app.Resume!, extracted);
            }

            var match = await matchTask;
            if (!match.IsSuccess)
            {
                lastError = match.ErrorMessage;
                continue;
            }

            ApplyMatchResult(app, match);
            scored++;
        }

        if (scored > 0)
        {
            await _context.SaveChangesAsync();
        }

        return (scored, scored == 0 ? lastError : null);
    }

    public async Task<(bool ok, string? error, int score, string? verdict)> ScoreApplicantAsync(int applicationId, int recruiterId)
    {
        var app = await _context.Set<Domain.Entities.Application>()
            .Include(a => a.JobPosting)
            .Include(a => a.Resume)
            .FirstOrDefaultAsync(a => a.Id == applicationId && a.JobPosting!.Company!.RecruiterId == recruiterId);

        if (app == null || app.JobPosting == null)
        {
            return (false, ErrorMessage.ApplicationNotFound, 0, null);
        }

        var cvText = app.Resume?.RawTextContent;
        if (string.IsNullOrWhiteSpace(cvText)) cvText = app.CoverLetter;
        if (string.IsNullOrWhiteSpace(cvText))
        {
            return (false, ErrorMessage.CvContentMissing, 0, null);
        }

        // Chạy song song: trích xuất CV (nếu cần) + chấm điểm theo JD -> nhanh gần gấp đôi
        var needExtract = app.Resume != null && app.Resume.AiAnalyzedAt == null
            && !string.IsNullOrWhiteSpace(app.Resume.RawTextContent);
        var extractTask = needExtract
            ? _aiService.ExtractCvInfoAsync(app.Resume!.RawTextContent!)
            : null;
        var matchTask = _aiService.MatchCvAsync(
            app.JobPosting.Title, app.JobPosting.Description, app.JobPosting.Requirements, cvText, BuildSettings(app.JobPosting));

        if (extractTask != null)
        {
            var extracted = await extractTask;
            if (extracted.IsSuccess) ApplyExtractToResume(app.Resume!, extracted);
        }

        var match = await matchTask;

        if (!match.IsSuccess)
        {
            return (false, match.ErrorMessage ?? ErrorMessage.AiEvaluationError, 0, null);
        }

        ApplyMatchResult(app, match);
        await _context.SaveChangesAsync();
        return (true, null, match.MatchScore, match.Verdict);
    }

    public async Task<bool> SaveJdSettingsAsync(int jobId, int recruiterId, JdEvalSettings settings)
    {
        var job = await _context.JobPostings
            .FirstOrDefaultAsync(j => j.Id == jobId && j.Company!.RecruiterId == recruiterId);
        if (job == null) return false;

        job.AiWeightExperience = Math.Clamp(settings.WeightExperience, 0, 100);
        job.AiWeightSkills = Math.Clamp(settings.WeightSkills, 0, 100);
        job.AiWeightEducation = Math.Clamp(settings.WeightEducation, 0, 100);
        job.AiWeightAchievement = Math.Clamp(settings.WeightAchievement, 0, 100);
        job.AiPriorityNote = string.IsNullOrWhiteSpace(settings.PriorityNote) ? null : settings.PriorityNote.Trim();

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<(bool ok, string? error)> ApplyAsync(int jobId, int candidateId, string fileName, string filePath, byte[] pdfBytes, string? coverLetter)
    {
        var job = await _context.JobPostings.FirstOrDefaultAsync(j => j.Id == jobId);
        if (job == null || job.Status != JobStatus.Active)
        {
            return (false, "Tin tuyển dụng không tồn tại hoặc đã đóng.");
        }

        var already = await _context.Set<Domain.Entities.Application>()
            .AnyAsync(a => a.JobPostingId == jobId && a.CandidateId == candidateId);
        if (already)
        {
            return (false, "Bạn đã ứng tuyển tin này rồi.");
        }

        // Đọc text từ PDF để phục vụ AI chấm điểm sau này
        string? rawText = null;
        try
        {
            using var ms = new MemoryStream(pdfBytes);
            rawText = PdfTextExtractor.Extract(ms);
        }
        catch
        {
            // Không đọc được text vẫn cho ứng tuyển; recruiter vẫn xem được file PDF
        }

        var resume = new Domain.Entities.Resume
        {
            CandidateId = candidateId,
            Title = fileName,
            FilePath = filePath,
            RawTextContent = rawText,
            IsDefault = false,
            CreatedAt = DateTime.UtcNow
        };
        _context.Resumes.Add(resume);
        await _context.SaveChangesAsync();

        var application = new Domain.Entities.Application
        {
            JobPostingId = jobId,
            CandidateId = candidateId,
            ResumeId = resume.Id,
            CoverLetter = string.IsNullOrWhiteSpace(coverLetter) ? null : coverLetter.Trim(),
            Status = ApplicationStatus.Pending,
            AppliedAt = DateTime.UtcNow
        };
        _context.Set<Domain.Entities.Application>().Add(application);
        await _context.SaveChangesAsync();

        return (true, null);
    }

    public async Task<bool> HasAppliedAsync(int jobId, int candidateId)
    {
        return await _context.Set<Domain.Entities.Application>()
            .AnyAsync(a => a.JobPostingId == jobId && a.CandidateId == candidateId);
    }

    private static JdEvalSettings BuildSettings(Domain.Entities.JobPosting job) => new()
    {
        WeightExperience = job.AiWeightExperience,
        WeightSkills = job.AiWeightSkills,
        WeightEducation = job.AiWeightEducation,
        WeightAchievement = job.AiWeightAchievement,
        PriorityNote = job.AiPriorityNote
    };

    private static void ApplyMatchResult(Domain.Entities.Application app, CvMatchResult match)
    {
        app.AiMatchScore = match.MatchScore;
        app.AiFeedback = string.IsNullOrWhiteSpace(match.Summary) ? match.Feedback : match.Summary;
        app.AiVerdict = match.Verdict;
        app.AiMatchedSkills = JoinLines(match.MatchedSkills);
        app.AiMissingSkills = JoinLines(match.MissingSkills);
        app.AiStrengths = JoinLines(match.Strengths);
        app.AiConcerns = JoinLines(match.Concerns);
        app.AiRecommendation = match.Recommendation;
        app.AiScoredAt = DateTime.UtcNow;
    }

    private static string? JoinLines(List<string> items) =>
        items.Count > 0 ? string.Join("\n", items) : null;

    private static List<string> SplitLines(string? s) =>
        string.IsNullOrWhiteSpace(s)
            ? new List<string>()
            : s.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
}
