using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using System.Security.Claims;

namespace WebApp.Controllers;

[Authorize(Policy = "CanApplyJob")]
public class CandidateController : Controller
{
    private readonly IApplicationService _applicationService;
    private readonly IWebHostEnvironment _env;
    private readonly INotificationService _notificationService;

    public CandidateController(IApplicationService applicationService, IWebHostEnvironment env, INotificationService notificationService)
    {
        _applicationService = applicationService;
        _env = env;
        _notificationService = notificationService;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<IActionResult> Apply(int jobId, IFormFile cvFile, string? coverLetter)
    {
        if (cvFile == null || cvFile.Length == 0)
        {
            TempData["ErrorMessage"] = "Vui lòng chọn file CV.";
            return RedirectToAction("Detail", "Job", new { id = jobId });
        }

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdString, out int candidateId))
        {
            return RedirectToAction("Login", "Auth");
        }

        var uploadFolder = Path.Combine(_env.WebRootPath, "uploads", "cvs");
        if (!Directory.Exists(uploadFolder))
        {
            Directory.CreateDirectory(uploadFolder);
        }

        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(cvFile.FileName)}";
        var filePath = Path.Combine(uploadFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await cvFile.CopyToAsync(stream);
        }

        var dbFilePath = $"/uploads/cvs/{fileName}";

        var result = await _applicationService.ApplyJobAsync(candidateId, jobId, dbFilePath, cvFile.FileName, coverLetter);

        if (result)
        {
            TempData["SuccessMessage"] = "CV của bạn đã được gửi đi!";
            return RedirectToAction(nameof(MyApplications));
        }
        else
        {
            TempData["ErrorMessage"] = "Bạn đã ứng tuyển công việc này rồi hoặc công việc không còn hợp lệ.";
            return RedirectToAction("Detail", "Job", new { id = jobId });
        }
    }

    [HttpGet]
    public async Task<IActionResult> MyApplications()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdString, out int candidateId))
        {
            return RedirectToAction("Login", "Auth");
        }

        var applications = await _applicationService.GetMyApplicationsAsync(candidateId);
        return View(applications);
    }

    [HttpPost]
    public async Task<IActionResult> Withdraw(int applicationId, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            TempData["ErrorMessage"] = "Vui lòng nhập lý do từ chối.";
            return RedirectToAction(nameof(MyApplications));
        }

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdString, out int candidateId))
        {
            return RedirectToAction("Login", "Auth");
        }

        var result = await _applicationService.WithdrawApplicationAsync(candidateId, applicationId, reason);
        if (result)
        {
            TempData["SuccessMessage"] = "Hủy đơn ứng tuyển thành công.";
        }
        else
        {
            TempData["ErrorMessage"] = "Không thể hủy đơn ứng tuyển này.";
        }

        return RedirectToAction(nameof(MyApplications));
    }

    // ====== NOTIFICATION ACTIONS ======

    [HttpGet]
    public async Task<IActionResult> Notifications()
    {
        var notifications = await _notificationService.GetByUserAsync(CurrentUserId);
        return View(notifications);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(int id)
    {
        await _notificationService.MarkAsReadAsync(id, CurrentUserId);
        return RedirectToAction(nameof(Notifications));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllRead()
    {
        await _notificationService.MarkAllAsReadAsync(CurrentUserId);
        return RedirectToAction(nameof(Notifications));
    }

    [HttpGet]
    public async Task<IActionResult> GetUnreadCount()
    {
        var count = await _notificationService.GetUnreadCountAsync(CurrentUserId);
        return Json(new { count });
    }
}
