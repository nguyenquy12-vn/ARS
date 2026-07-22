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

    public CandidateController(IApplicationService applicationService, IWebHostEnvironment env)
    {
        _applicationService = applicationService;
        _env = env;
    }

    [HttpPost]
    public async Task<IActionResult> Apply(int jobId, IFormFile cvFile, string? coverLetter)
    {
        if (cvFile == null || cvFile.Length == 0)
        {
            TempData["ErrorMessage"] = "Vui lòng chọn file CV.";
            return RedirectToAction("Detail", "Job", new { id = jobId });
        }

        // Lấy CandidateId từ Cookie (ClaimTypes.NameIdentifier)
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdString, out int candidateId))
        {
            return RedirectToAction("Login", "Auth");
        }

        // Tạo thư mục nếu chưa có
        var uploadFolder = Path.Combine(_env.WebRootPath, "uploads", "cvs");
        if (!Directory.Exists(uploadFolder))
        {
            Directory.CreateDirectory(uploadFolder);
        }

        // Đổi tên file để tránh trùng lặp
        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(cvFile.FileName)}";
        var filePath = Path.Combine(uploadFolder, fileName);

        // Lưu file vật lý
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await cvFile.CopyToAsync(stream);
        }

        // Đường dẫn ảo lưu vào DB
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
}
