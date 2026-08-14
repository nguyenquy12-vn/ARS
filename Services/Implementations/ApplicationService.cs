using Domain.Entities;
using Domain.Constraints;
using Domain.Enums;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Services.DTOs.Application;
using Services.Interfaces;

namespace Services.Implementations;

// [BẢO VỆ] APPLICATION SERVICE: ứng tuyển/rút đơn, pipeline, AI, lịch phỏng vấn và email.
public class ApplicationService : IApplicationService
{
    private readonly ARSDbContext _context;
    private readonly IAiService _aiService;
    private readonly IEmailService _emailService;
    private readonly INotificationService _notificationService;

    public ApplicationService(ARSDbContext context, IAiService aiService, IEmailService emailService, INotificationService notificationService)
    {
        _context = context;
        _aiService = aiService;
        _emailService = emailService;
        _notificationService = notificationService;
    }

    public async Task<bool> ApplyJobAsync(int candidateId, int jobId, string cvFilePath, string cvFileName, string? coverLetter)
    {
        // 1. Kiểm tra việc làm có tồn tại và còn hạn không
        var job = await _context.JobPostings
            .Include(j => j.Company)
            .FirstOrDefaultAsync(j => j.Id == jobId);
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

        // 5. Gửi thông báo cho Recruiter
        var recruiterId = job.Company?.RecruiterId;
        if (recruiterId.HasValue)
        {
            var candidate = await _context.Users.FirstOrDefaultAsync(u => u.Id == candidateId);
            var candidateName = candidate?.FullName ?? "Ứng viên";
            var jobTitle = job.Title;

            await _notificationService.CreateAsync(
                recruiterId.Value,
                $"📄 CV mới: {jobTitle}",
                $"{candidateName} vừa nộp CV ứng tuyển vị trí {jobTitle}. Hãy vào xem xét hồ sơ ngay!",
                "NewApplication",
                job.Id
            );
        }

        return true;
    }

    public async Task<List<ApplicationDto>> GetMyApplicationsAsync(int candidateId)
    {
        var applications = await _context.Set<Application>()
            .Include(a => a.JobPosting)
            .ThenInclude(j => j!.Company)
            .Where(a => a.CandidateId == candidateId)
            .OrderByDescending(a => a.AppliedAt)
            .Select(a => new ApplicationDto
            {
                Id = a.Id,
                JobPostingId = a.JobPostingId,
                JobTitle = a.JobPosting!.Title,
                CompanyName = a.JobPosting.Company!.CompanyName,
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
        if (application.Status == ApplicationStatus.Accepted ||
            application.Status == ApplicationStatus.Rejected ||
            application.Status == ApplicationStatus.Withdrawn)
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
            JobTitle = a.JobPosting?.Title ?? string.Empty,
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
            ResumeFilePath = a.Resume?.FilePath,
            InterviewAt = a.InterviewAt,
            InterviewNote = a.InterviewNote
        }).ToList();
    }

    public async Task<List<ApplicantListItem>> GetUpcomingInterviewsAsync(int recruiterId)
    {
        return await _context.Set<Domain.Entities.Application>()
            .Include(application => application.Candidate)
            .Include(application => application.JobPosting)
            .Where(application => application.JobPosting!.Company!.RecruiterId == recruiterId
                && application.InterviewAt.HasValue
                && application.Status == ApplicationStatus.Interview)
            .OrderBy(application => application.InterviewAt)
            .Select(application => new ApplicantListItem
            {
                Id = application.Id,
                JobTitle = application.JobPosting!.Title,
                CandidateName = application.Candidate != null ? application.Candidate.FullName : string.Empty,
                CandidateEmail = application.Candidate != null ? application.Candidate.Email : string.Empty,
                Status = application.Status,
                InterviewAt = application.InterviewAt,
                InterviewNote = application.InterviewNote,
                AiMatchScore = application.AiMatchScore
            })
            .ToListAsync();
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
            .Include(a => a.Candidate)
            .Include(a => a.JobPosting)
            .FirstOrDefaultAsync(a => a.Id == applicationId && a.JobPosting!.Company!.RecruiterId == recruiterId);

        if (application == null)
        {
            return ApplicationResult.Failure(ErrorMessage.ApplicationNotFound);
        }

        var wasAccepted = application.Status == ApplicationStatus.Accepted;
        if (wasAccepted && status != ApplicationStatus.Accepted)
        {
            return ApplicationResult.Failure(ErrorMessage.AcceptedApplicationFinal);
        }

        var isBeingAccepted = status == ApplicationStatus.Accepted && !wasAccepted;

        // "Vacancies" là số lượng còn cần tuyển. Chỉ giảm đúng một lần khi
        // ứng viên lần đầu được xác nhận Đạt/đi làm.
        if (isBeingAccepted)
        {
            if (application.JobPosting == null || application.JobPosting.Vacancies <= 0)
            {
                return ApplicationResult.Failure(ErrorMessage.JobHasNoVacancy);
            }

            application.JobPosting.Vacancies--;
        }

        application.Status = status;

        try
        {
            await _context.SaveChangesAsync();

            // Gửi email thông báo khi Đạt / Từ chối
            if (status is ApplicationStatus.Accepted or ApplicationStatus.Rejected)
            {
                await NotifyStatusAsync(application, recruiterId, status);
            }

            return ApplicationResult.Success(application.Id, application.JobPostingId);
        }
        catch (Exception)
        {
            return ApplicationResult.Failure(ErrorMessage.ApplicationSaveError);
        }
    }

    public async Task<(bool ok, string? error, string? mailInfo)> ScheduleInterviewAsync(int applicationId, int recruiterId, DateTime interviewAt, string? note)
    {
        var application = await _context.Set<Domain.Entities.Application>()
            .Include(a => a.Candidate)
            .Include(a => a.JobPosting)
            .FirstOrDefaultAsync(a => a.Id == applicationId && a.JobPosting!.Company!.RecruiterId == recruiterId);

        if (application == null)
        {
            return (false, ErrorMessage.ApplicationNotFound, null);
        }

        application.InterviewAt = interviewAt;
        application.InterviewNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        application.Status = ApplicationStatus.Interview;
        await _context.SaveChangesAsync();

        var jobTitle = application.JobPosting?.Title ?? "vị trí ứng tuyển";
        var candidateId = application.CandidateId;
        var timeStr = interviewAt.ToString("HH:mm - dd/MM/yyyy");

        // Tạo thông báo nội bộ cho ứng viên
        var notifMsg = $"Bạn được mời phỏng vấn cho vị trí <strong>{jobTitle}</strong>.<br/>" +
                       $"🕐 Thời gian: <strong>{timeStr}</strong>" +
                       (string.IsNullOrWhiteSpace(note) ? "" : $"<br/>📍 Địa điểm / Link: <strong>{note}</strong>");
        await _notificationService.CreateAsync(candidateId, $"📅 Mời phỏng vấn: {jobTitle}", notifMsg, "Interview", application.Id);

        // Gửi email mời phỏng vấn
        var recruiter = await _context.Users.FirstOrDefaultAsync(u => u.Id == recruiterId);
        var candidateEmail = application.Candidate?.Email;
        if (recruiter == null || string.IsNullOrWhiteSpace(candidateEmail))
        {
            return (true, null, "Đã lưu lịch nhưng chưa gửi được email (thiếu thông tin).");
        }

        var subject = $"[Mời phỏng vấn] {jobTitle}";
        var body = $@"
<p>Xin chào <strong>{application.Candidate?.FullName}</strong>,</p>
<p>Bạn được mời tham gia phỏng vấn cho vị trí <strong>{jobTitle}</strong>.</p>
<ul>
  <li><strong>Thời gian:</strong> {interviewAt:HH:mm dddd, dd/MM/yyyy}</li>
  {(string.IsNullOrWhiteSpace(note) ? "" : $"<li><strong>Địa điểm / Ghi chú:</strong> {note}</li>")}
</ul>
<p>Vui lòng phản hồi email này để xác nhận. Trân trọng,<br/>{recruiter.FullName}</p>";

        var (ok, error) = await _emailService.SendAsync(recruiter, candidateEmail, subject, body);
        var mailInfo = ok ? $"Đã gửi email mời phỏng vấn tới {candidateEmail}." : $"Đã lưu lịch. Gửi email lỗi: {error}";
        return (true, null, mailInfo);
    }

    public async Task<(int ok, int failed, string info)> BulkScheduleInterviewAsync(int jobId, int recruiterId, IEnumerable<int> applicationIds, DateTime interviewAt, string? note)
    {
        var ids = applicationIds?.Distinct().ToList() ?? new List<int>();
        if (ids.Count == 0) return (0, 0, "Chưa chọn ứng viên nào.");

        var apps = await _context.Set<Domain.Entities.Application>()
            .Include(a => a.Candidate)
            .Include(a => a.JobPosting)
            .Where(a => ids.Contains(a.Id)
                && a.JobPostingId == jobId
                && a.JobPosting!.Company!.RecruiterId == recruiterId)
            .ToListAsync();

        if (apps.Count == 0) return (0, 0, "Không tìm thấy ứng viên hợp lệ.");

        var trimmedNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        foreach (var app in apps)
        {
            app.InterviewAt = interviewAt;
            app.InterviewNote = trimmedNote;
            app.Status = ApplicationStatus.Interview;
        }
        await _context.SaveChangesAsync();

        // Tạo thông báo nội bộ cho từng ứng viên được mời
        var timeStr = interviewAt.ToString("HH:mm - dd/MM/yyyy");
        foreach (var app in apps)
        {
            var jt = app.JobPosting?.Title ?? "vị trí ứng tuyển";
            var notifMsg = $"Bạn được mời phỏng vấn cho vị trí <strong>{jt}</strong>.<br/>" +
                           $"🕐 Thời gian: <strong>{timeStr}</strong>" +
                           (string.IsNullOrWhiteSpace(trimmedNote) ? "" : $"<br/>📍 Địa điểm / Link: <strong>{trimmedNote}</strong>");
            await _notificationService.CreateAsync(app.CandidateId, $"📅 Mời phỏng vấn: {jt}", notifMsg, "Interview", app.Id);
        }

        var recruiter = await _context.Users.FirstOrDefaultAsync(u => u.Id == recruiterId);
        int ok = 0, failed = 0;
        if (recruiter != null)
        {
            foreach (var app in apps)
            {
                var email = app.Candidate?.Email;
                if (string.IsNullOrWhiteSpace(email)) { failed++; continue; }

                var jobTitle = app.JobPosting?.Title ?? "vị trí ứng tuyển";
                var subject = $"[Mời phỏng vấn] {jobTitle}";
                var body = $@"
<p>Xin chào <strong>{app.Candidate?.FullName}</strong>,</p>
<p>Bạn được mời tham gia phỏng vấn cho vị trí <strong>{jobTitle}</strong>.</p>
<ul>
  <li><strong>Thời gian:</strong> {interviewAt:HH:mm dddd, dd/MM/yyyy}</li>
  {(string.IsNullOrWhiteSpace(trimmedNote) ? "" : $"<li><strong>Địa điểm / Ghi chú:</strong> {trimmedNote}</li>")}
</ul>
<p>Vui lòng phản hồi email này để xác nhận. Trân trọng,<br/>{recruiter.FullName}</p>";

                var (sent, _) = await _emailService.SendAsync(recruiter, email, subject, body);
                if (sent) ok++; else failed++;
            }
        }

        var info = $"Đã lưu lịch PV cho {apps.Count} ứng viên. Email: {ok} gửi thành công"
            + (failed > 0 ? $", {failed} lỗi/thiếu email." : ".");
        return (ok, failed, info);
    }

    private async Task NotifyStatusAsync(Domain.Entities.Application application, int recruiterId, ApplicationStatus status)
    {
        var recruiter = await _context.Users.FirstOrDefaultAsync(u => u.Id == recruiterId);
        var candidateEmail = application.Candidate?.Email;
        var jobTitle = application.JobPosting?.Title ?? "vị trí ứng tuyển";

        // Tạo thông báo nội bộ cho ứng viên
        if (status == ApplicationStatus.Accepted)
        {
            await _notificationService.CreateAsync(
                application.CandidateId,
                $"✅ Chúc mừng! Hồ sơ được chấp nhận",
                $"Hồ sơ của bạn ứng tuyển vị trí <strong>{jobTitle}</strong> đã được <strong>chấp nhận</strong>. Nhà tuyển dụng sẽ sớm liên hệ với bạn.",
                "StatusAccepted", application.Id);
        }
        else if (status == ApplicationStatus.Rejected)
        {
            await _notificationService.CreateAsync(
                application.CandidateId,
                $"❌ Kết quả ứng tuyển: {jobTitle}",
                $"Rất tiếc, hồ sơ của bạn ứng tuyển vị trí <strong>{jobTitle}</strong> chưa phù hợp ở thời điểm này. Chúc bạn sớm tìm được công việc ưng ý!",
                "StatusRejected", application.Id);
        }

        // Gửi email thông báo
        if (recruiter == null || string.IsNullOrWhiteSpace(candidateEmail)) return;

        string subject, body;
        if (status == ApplicationStatus.Accepted)
        {
            subject = $"[Kết quả ứng tuyển] Chúc mừng - {jobTitle}";
            body = $@"<p>Xin chào <strong>{application.Candidate?.FullName}</strong>,</p>
<p>Chúc mừng! Hồ sơ của bạn cho vị trí <strong>{jobTitle}</strong> đã được chấp nhận. Chúng tôi sẽ liên hệ với bạn trong thời gian sớm nhất.</p>
<p>Trân trọng,<br/>{recruiter.FullName}</p>";
        }
        else
        {
            subject = $"[Kết quả ứng tuyển] {jobTitle}";
            body = $@"<p>Xin chào <strong>{application.Candidate?.FullName}</strong>,</p>
<p>Cảm ơn bạn đã ứng tuyển vị trí <strong>{jobTitle}</strong>. Rất tiếc, hồ sơ của bạn chưa phù hợp ở thời điểm này. Chúc bạn sớm tìm được công việc phù hợp.</p>
<p>Trân trọng,<br/>{recruiter.FullName}</p>";
        }

        await _emailService.SendAsync(recruiter, candidateEmail, subject, body);
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

        // Lọc các đơn cần chấm
        var toScore = applications
            .Where(app => rescoreAll || app.AiScoredAt == null)
            .Select(app =>
            {
                var cvText = app.Resume?.RawTextContent;
                if (string.IsNullOrWhiteSpace(cvText)) cvText = app.CoverLetter;
                return new { App = app, CvText = cvText };
            })
            .Where(x => !string.IsNullOrWhiteSpace(x.CvText))
            .ToList();

        // Gọi AI SONG SONG tối đa 4 luồng (chỉ phần mạng, KHÔNG chạm DbContext).
        var gate = new SemaphoreSlim(4);
        var aiTasks = toScore.Select(async x =>
        {
            await gate.WaitAsync();
            try
            {
                var needExtract = x.App.Resume != null && x.App.Resume.AiAnalyzedAt == null
                    && !string.IsNullOrWhiteSpace(x.App.Resume.RawTextContent);
                var extractTask = needExtract ? _aiService.ExtractCvInfoAsync(x.App.Resume!.RawTextContent!) : null;
                var matchTask = _aiService.MatchCvAsync(job.Title, job.Description, job.Requirements, x.CvText!, settings);

                var extracted = extractTask != null ? await extractTask : null;
                var match = await matchTask;
                return new { x.App, Extracted = extracted, Match = match };
            }
            finally
            {
                gate.Release();
            }
        });

        var results = await Task.WhenAll(aiTasks);

        // Ghi kết quả vào DbContext TUẦN TỰ (thread-safe).
        foreach (var r in results)
        {
            if (r.Extracted != null && r.Extracted.IsSuccess)
                ApplyExtractToResume(r.App.Resume!, r.Extracted);

            if (!r.Match.IsSuccess)
            {
                lastError = r.Match.ErrorMessage;
                continue;
            }

            ApplyMatchResult(r.App, r.Match);
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
        var job = await _context.JobPostings
            .Include(j => j.Company)
            .FirstOrDefaultAsync(j => j.Id == jobId);
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

        // Đây là luồng nộp CV đang dùng ở trang /Job/Detail.
        // Tạo thông báo cho đúng recruiter sở hữu công ty của tin tuyển dụng.
        var recruiterId = job.Company?.RecruiterId;
        if (recruiterId.HasValue)
        {
            var candidateName = await _context.Users
                .Where(user => user.Id == candidateId)
                .Select(user => user.FullName)
                .FirstOrDefaultAsync() ?? "Ứng viên";

            await _notificationService.CreateAsync(
                recruiterId.Value,
                $"CV mới: {job.Title}",
                $"{candidateName} vừa nộp CV cho vị trí {job.Title}. Hãy xem và xử lý hồ sơ.",
                "NewApplication",
                job.Id);
        }

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
