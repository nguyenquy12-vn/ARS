using System.Security.Claims;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.DTOs.Application;
using Services.Interfaces;
using WebApp.Models.Application;

namespace WebApp.Controllers;

[Authorize]
public class ApplicationController : Controller
{
    private readonly IApplicationService _applicationService;
    private readonly IJobPostingService _jobPostingService;

    public ApplicationController(IApplicationService applicationService, IJobPostingService jobPostingService)
    {
        _applicationService = applicationService;
        _jobPostingService = jobPostingService;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // GET /Application/ByJob/5  -> danh sách ứng viên của một tin tuyển dụng
    [HttpGet]
    [Authorize(Policy = "CanReviewCV")]
    public async Task<IActionResult> ByJob(int id)
    {
        var job = await _jobPostingService.GetForRecruiterAsync(id, CurrentUserId);
        if (job == null)
        {
            return NotFound();
        }

        var applicants = await _applicationService.GetApplicantsForJobAsync(id, CurrentUserId);
        return View(new JobApplicantsViewModel { Job = job, Applicants = applicants });
    }

    // POST /Application/Analyze -> trích xuất thông tin CV bằng AI cho các ứng viên chưa phân tích
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CanReviewCV")]
    public async Task<IActionResult> Analyze(int id)
    {
        try
        {
            var count = await _applicationService.AnalyzeApplicantsAsync(id, CurrentUserId);
            TempData["Success"] = count > 0
                ? $"AI đã phân tích {count} CV."
                : "Không có CV mới để phân tích (hoặc AI không phản hồi).";
        }
        catch (Exception)
        {
            TempData["Error"] = "Không kết nối được máy chủ AI. Vui lòng kiểm tra 'Ai:BaseUrl' trong appsettings.json.";
        }

        return RedirectToAction(nameof(ByJob), new { id });
    }

    // POST /Application/ScoreAll -> chấm điểm ứng viên theo JD (rescore=false: chưa chấm; true: chấm lại tất cả)
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CanEvaluateAI")]
    public async Task<IActionResult> ScoreAll(int id, bool rescore = false)
    {
        try
        {
            var (scored, error) = await _applicationService.ScoreApplicantsAsync(id, CurrentUserId, rescore);
            if (scored > 0)
            {
                TempData["Success"] = $"AI đã chấm điểm {scored} ứng viên theo JD.";
            }
            else
            {
                TempData["Error"] = error ?? "Không có ứng viên nào cần chấm (hoặc AI không phản hồi).";
            }
        }
        catch (Exception)
        {
            TempData["Error"] = "Không kết nối được máy chủ AI. Vui lòng kiểm tra 'Ai:BaseUrl' trong appsettings.json.";
        }

        return RedirectToAction(nameof(ByJob), new { id });
    }

    // POST /Application/ScoreOne -> chấm điểm 1 ứng viên (trả JSON, dùng cho thanh tiến trình)
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CanEvaluateAI")]
    public async Task<IActionResult> ScoreOne(int id)
    {
        try
        {
            var (ok, error, score, verdict) = await _applicationService.ScoreApplicantAsync(id, CurrentUserId);
            return Json(new { ok, error, score, verdict });
        }
        catch (Exception)
        {
            return Json(new { ok = false, error = "Không kết nối được máy chủ AI.", score = 0, verdict = (string?)null });
        }
    }

    // POST /Application/SaveScoreSettings -> lưu trọng số/ưu tiên chấm điểm cho tin tuyển dụng
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CanReviewCV")]
    public async Task<IActionResult> SaveScoreSettings(int id, JdEvalSettings settings)
    {
        var ok = await _applicationService.SaveJdSettingsAsync(id, CurrentUserId, settings);
        TempData[ok ? "Success" : "Error"] = ok
            ? "Đã lưu cài đặt chấm điểm cho tin này."
            : "Không lưu được cài đặt.";
        return RedirectToAction(nameof(ByJob), new { id });
    }

    // GET /Application/Details/5 -> chi tiết một hồ sơ ứng tuyển
    [HttpGet]
    [Authorize(Policy = "CanReviewCV")]
    public async Task<IActionResult> Details(int id)
    {
        var detail = await _applicationService.GetDetailAsync(id, CurrentUserId);
        if (detail == null)
        {
            return NotFound();
        }

        return View(detail);
    }

    // POST /Application/SetStatus -> cập nhật trạng thái duyệt hồ sơ
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CanReviewCV")]
    public async Task<IActionResult> SetStatus(int id, ApplicationStatus status)
    {
        var result = await _applicationService.UpdateStatusAsync(id, CurrentUserId, status);

        if (result.IsSuccess)
        {
            TempData["Success"] = "Đã cập nhật trạng thái hồ sơ.";
        }
        else
        {
            TempData["Error"] = result.ErrorMessage;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    // POST /Application/Evaluate -> chấm điểm CV bằng Gemini AI
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CanEvaluateAI")]
    public async Task<IActionResult> Evaluate(int id)
    {
        var result = await _applicationService.EvaluateWithAiAsync(id, CurrentUserId);

        if (result.IsSuccess)
        {
            TempData["Success"] = $"AI đã chấm điểm CV: {result.AiMatchScore}/100.";
        }
        else
        {
            TempData["Error"] = result.ErrorMessage;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    // POST /Application/ScheduleInterview -> hẹn lịch phỏng vấn + gửi email mời
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CanReviewCV")]
    public async Task<IActionResult> ScheduleInterview(int id, int jobId, DateTime interviewAt, string? note)
    {
        var (ok, error, mailInfo) = await _applicationService.ScheduleInterviewAsync(id, CurrentUserId, interviewAt, note);
        if (ok)
        {
            TempData["Success"] = mailInfo ?? "Đã lưu lịch phỏng vấn.";
        }
        else
        {
            TempData["Error"] = error;
        }
        return RedirectToAction(nameof(ByJob), new { id = jobId });
    }
}
