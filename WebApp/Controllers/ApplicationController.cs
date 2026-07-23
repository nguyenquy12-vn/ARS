using System.Security.Claims;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using WebApp.Models.Application;

namespace WebApp.Controllers;

[Authorize(Roles = "Recruiter")]
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
}
