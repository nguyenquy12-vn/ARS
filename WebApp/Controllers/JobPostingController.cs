using System.Security.Claims;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Services.DTOs.JobPosting;
using Services.Interfaces;
using WebApp.Models.JobPosting;
using WebApp.Filters;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Domain.Enums;

namespace WebApp.Controllers;

[Authorize(Roles = "Recruiter")]
[RequireActiveRecruiterPlan]
public class JobPostingController : Controller
{
    private readonly IJobPostingService _jobPostingService;
    private readonly ARSDbContext _context;

    public JobPostingController(IJobPostingService jobPostingService, ARSDbContext context)
    {
        _jobPostingService = jobPostingService;
        _context = context;
    }

    // Lấy Id của người dùng đang đăng nhập từ Claim
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    [Authorize(Policy = "CanViewJob")]
    public async Task<IActionResult> Index()
    {
        var jobs = await _jobPostingService.GetRecruiterJobsAsync(CurrentUserId);
        return View(jobs);
    }

    [HttpGet]
    [Authorize(Policy = "CanViewJob")]
    public async Task<IActionResult> Details(int id)
    {
        var job = await _jobPostingService.GetForRecruiterAsync(id, CurrentUserId);
        if (job == null)
        {
            return NotFound();
        }

        return View(job);
    }

    [HttpGet]
    [Authorize(Policy = "CanCreateJob")]
    public async Task<IActionResult> Create()
    {
        if (!await CanCreateJobAsync())
        {
            TempData["Error"] = "Gói hiện tại đã dùng hết số bài đăng cho phép. Hãy đóng một bài hoặc nâng cấp gói.";
            return RedirectToAction(nameof(Index));
        }
        var model = new JobPostingFormViewModel();
        await PopulateCategoriesAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CanCreateJob")]
    public async Task<IActionResult> Create(JobPostingFormViewModel model)
    {
        if (!await CanCreateJobAsync())
        {
            TempData["Error"] = "Bạn đã dùng hết số bài đăng của gói hiện tại. Vui lòng đóng một bài hoặc nâng cấp lên Pro.";
            return RedirectToAction(nameof(Index));
        }
        if (!ModelState.IsValid)
        {
            await PopulateCategoriesAsync(model);
            return View(model);
        }

        var request = model.Adapt<CreateJobPostingRequest>();
        var result = await _jobPostingService.CreateAsync(CurrentUserId, request);

        if (result.IsSuccess)
        {
            TempData["Success"] = "Đăng tin tuyển dụng thành công.";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(string.Empty, result.ErrorMessage);
        await PopulateCategoriesAsync(model);
        return View(model);
    }

    [HttpGet]
    [Authorize(Policy = "CanEditJob")]
    public async Task<IActionResult> Edit(int id)
    {
        var job = await _jobPostingService.GetForRecruiterAsync(id, CurrentUserId);
        if (job == null)
        {
            return NotFound();
        }

        var model = job.Adapt<JobPostingFormViewModel>();
        await PopulateCategoriesAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CanEditJob")]
    public async Task<IActionResult> Edit(int id, JobPostingFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateCategoriesAsync(model);
            return View(model);
        }

        var request = model.Adapt<UpdateJobPostingRequest>();
        var result = await _jobPostingService.UpdateAsync(id, CurrentUserId, request);

        if (result.IsSuccess)
        {
            TempData["Success"] = "Cập nhật tin tuyển dụng thành công.";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(string.Empty, result.ErrorMessage);
        await PopulateCategoriesAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CanDeleteJob")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _jobPostingService.DeleteAsync(id, CurrentUserId);

        if (result.IsSuccess)
        {
            TempData["Success"] = "Đã xóa tin tuyển dụng.";
        }
        else
        {
            TempData["Error"] = result.ErrorMessage;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateCategoriesAsync(JobPostingFormViewModel model)
    {
        var categories = await _jobPostingService.GetCategoriesAsync();
        model.Categories = categories.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = c.Name
        });
    }

    private async Task<bool> CanCreateJobAsync()
    {
        var plan = await _context.RecruiterSubscriptions
            .Where(x => x.RecruiterId == CurrentUserId && x.ExpiresAt > DateTime.UtcNow)
            .Select(x => x.PlanCode)
            .FirstOrDefaultAsync();
        if (plan == "Pro") return true;
        if (plan is not ("Starter" or "Free")) return false;

        var usedSlots = await _context.JobPostings.CountAsync(x =>
            x.Company != null && x.Company.RecruiterId == CurrentUserId &&
            x.Status != JobStatus.Closed && x.Status != JobStatus.Archived);
        return usedSlots < (plan == "Free" ? 1 : 3);
    }
}
