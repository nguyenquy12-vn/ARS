using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace WebApp.Controllers.Admin;

[Route("admin/jobs")]
public class JobManagementController : Controller
{

    private readonly IJobPostingService _jobPostingService;

    public JobManagementController(IJobPostingService jobPostingService)
    {
        _jobPostingService = jobPostingService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var jobs = await _jobPostingService.GetAllJobsAsync();

        return View(jobs);
    }

    [HttpGet("details/{jobId}")]
    public async Task<IActionResult> Details(int jobId)
    {
        var result = await _jobPostingService.GetJobDetailsAsync(jobId);
        if (result.IsSuccess)
        {
            return View(result.Job);
        }

        ModelState.AddModelError(string.Empty, result.ErrorMessage);
        return RedirectToAction("Index"); ;
    }


}
