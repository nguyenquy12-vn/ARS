using Domain.Enums;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Services.Interfaces;
using System.Security.Claims;
using WebApp.Models.Job;

namespace WebApp.Controllers;

public class JobController : Controller
{
    private readonly IJobPostingService _jobPostingService;
    private readonly IApplicationService _applicationService;
    private readonly IWebHostEnvironment _env;
    private readonly ARSDbContext _context;

    public JobController(
        IJobPostingService jobPostingService,
        IApplicationService applicationService,
        IWebHostEnvironment env,
        ARSDbContext context)
    {
        _jobPostingService = jobPostingService;
        _applicationService = applicationService;
        _env = env;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? keyword, int? categoryId, JobType? jobType, WorkMode? workMode)
    {
        if (User.Identity?.IsAuthenticated == true && User.IsInRole("Recruiter"))
        {
            return RedirectToAction("Index", "JobPosting");
        }

        var jobs = await _jobPostingService.GetActiveJobsAsync(keyword, categoryId, jobType, workMode);
        var categories = await _context.JobCategories.ToListAsync();

        var viewModel = new JobListViewModel
        {
            Jobs = jobs,
            Categories = categories,
            Keyword = keyword,
            CategoryId = categoryId,
            JobType = jobType,
            WorkMode = workMode
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        if (User.Identity?.IsAuthenticated == true && User.IsInRole("Recruiter"))
        {
            return RedirectToAction("Index", "JobPosting");
        }

        var jobDetail = await _jobPostingService.GetJobDetailAsync(id);
        if (jobDetail == null)
        {
            return NotFound();
        }

        if (User.Identity?.IsAuthenticated == true && User.IsInRole("Candidate"))
        {
            var candidateId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            ViewBag.HasApplied = await _applicationService.HasAppliedAsync(id, candidateId);
        }

        return View(jobDetail);
    }

    [HttpPost]
    [Authorize(Roles = "Candidate")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<IActionResult> Apply(int id, IFormFile cvFile, string? coverLetter)
    {
        var candidateId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        if (cvFile == null || cvFile.Length == 0)
        {
            TempData["ApplyError"] = "Vui lòng chọn file CV (PDF).";
            return RedirectToAction(nameof(Detail), new { id });
        }

        if (cvFile.Length > 10 * 1024 * 1024)
        {
            TempData["ApplyError"] = "File quá lớn (tối đa 10MB).";
            return RedirectToAction(nameof(Detail), new { id });
        }

        byte[] bytes;
        using (var ms = new MemoryStream())
        {
            await cvFile.CopyToAsync(ms);
            bytes = ms.ToArray();
        }

        if (!IsPdfFile(cvFile.FileName, bytes))
        {
            TempData["ApplyError"] = "Chỉ chấp nhận file PDF hợp lệ.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "cv");
        Directory.CreateDirectory(uploadDir);
        var storedName = $"{Guid.NewGuid():N}.pdf";

        var (ok, error) = await _applicationService.ApplyAsync(
            id, candidateId, Path.GetFileName(cvFile.FileName), $"/uploads/cv/{storedName}", bytes, coverLetter);

        if (ok)
        {
            await System.IO.File.WriteAllBytesAsync(Path.Combine(uploadDir, storedName), bytes);
            TempData["ApplySuccess"] = "Nộp hồ sơ ứng tuyển thành công!";
        }
        else
        {
            TempData["ApplyError"] = error;
        }

        return RedirectToAction(nameof(Detail), new { id });
    }

    private static bool IsPdfFile(string fileName, byte[] bytes)
    {
        if (!fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) || bytes.Length < 5)
        {
            return false;
        }

        return bytes[0] == 0x25
            && bytes[1] == 0x50
            && bytes[2] == 0x44
            && bytes[3] == 0x46
            && bytes[4] == 0x2D;
    }
}
