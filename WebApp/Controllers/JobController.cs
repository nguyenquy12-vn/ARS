using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Services.Interfaces;
using Infrastructure;
using Domain.Enums;
using WebApp.Models.Job;
using System.Threading.Tasks;

namespace WebApp.Controllers;

public class JobController : Controller
{
    private readonly IJobPostingService _jobPostingService;
    private readonly ARSDbContext _context;

    public JobController(IJobPostingService jobPostingService, ARSDbContext context)
    {
        _jobPostingService = jobPostingService;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? keyword, int? categoryId, JobType? jobType, WorkMode? workMode)
    {
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
        var jobDetail = await _jobPostingService.GetJobDetailAsync(id);
        if (jobDetail == null)
        {
            return NotFound();
        }

        return View(jobDetail);
    }
}
