using Domain.Enums;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Services.Interfaces;

namespace WebApp.Controllers.Admin;

// [BẢO VỆ] ADMIN BÀI ĐĂNG: CloseJob đổi Status=Closed, không xóa để giữ hồ sơ/lịch sử.
[Route("admin/jobs")]
[Authorize(Roles = "Admin")]
public class JobManagementController : Controller
{
    private readonly IJobPostingService _jobPostingService;
    private readonly ARSDbContext _context;

    public JobManagementController(IJobPostingService jobPostingService, ARSDbContext context)
    {
        _jobPostingService = jobPostingService;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var jobs = await _jobPostingService.GetAllJobsAsync();
        return View(jobs);
    }

    [HttpGet("details/{jobId:int}")]
    public async Task<IActionResult> Details(int jobId)
    {
        var result = await _jobPostingService.GetJobDetailsAsync(jobId);
        if (result.IsSuccess) return View(result.Job);
        TempData["Error"] = result.ErrorMessage;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("close/{jobId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CloseJob(int jobId)
    {
        var job = await _context.JobPostings.FirstOrDefaultAsync(x => x.Id == jobId);
        if (job is null)
        {
            TempData["Error"] = "Không tìm thấy bài đăng cần gỡ.";
            return RedirectToAction(nameof(Index));
        }

        if (job.Status != JobStatus.Active)
        {
            TempData["Error"] = "Chỉ có thể gỡ bài đăng đang hoạt động.";
            return RedirectToAction(nameof(Index));
        }

        job.Status = JobStatus.Closed;
        await _context.SaveChangesAsync();
        TempData["Success"] = $"Đã gỡ bài đăng “{job.Title}”. Hồ sơ ứng tuyển liên quan vẫn được giữ nguyên.";
        return RedirectToAction(nameof(Index));
    }
}
