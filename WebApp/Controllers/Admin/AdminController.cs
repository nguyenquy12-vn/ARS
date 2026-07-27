using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Models.Admin;

namespace WebApp.Controllers.Admin;

[Route("admin")]
[Microsoft.AspNetCore.Authorization.Authorize(Policy = "CanManageUsers")]
public class AdminController : Controller
{
    private readonly ARSDbContext _context;

    public AdminController(ARSDbContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    [HttpGet("index")]
    public async Task<IActionResult> Index()
    {
        var vm = new AdminDashboardViewModel();
        // Defensive: DB may be missing some tables (migrations not applied). Catch exceptions and degrade gracefully.
        try
        {
            vm.TotalUsers = await _context.Users.CountAsync();
        }
        catch
        {
            vm.TotalUsers = 0;
        }

        try
        {
            vm.TotalJobs = await _context.JobPostings.CountAsync();
        }
        catch
        {
            vm.TotalJobs = 0;
        }

        try
        {
            vm.TotalCompanies = await _context.Companies.CountAsync();
        }
        catch
        {
            vm.TotalCompanies = 0;
        }

        try
        {
            vm.TotalApplications = await _context.Applications.CountAsync();
        }
        catch
        {
            vm.TotalApplications = 0;
        }

        try
        {
            vm.RecentUsers = await _context.Users.OrderByDescending(u => u.CreatedAt).Take(5).Select(u => new RecentUser { Id = u.Id, Email = u.Email, FullName = u.FullName, CreatedAt = u.CreatedAt }).ToListAsync();
        }
        catch
        {
            vm.RecentUsers = new List<RecentUser>();
        }

        try
        {
            vm.RecentApplications = await _context.Applications.OrderByDescending(a => a.AppliedAt).Take(5).Select(a => new RecentApplication { Id = a.Id, CandidateId = a.CandidateId, JobPostingId = a.JobPostingId, AppliedAt = a.AppliedAt }).ToListAsync();
        }
        catch
        {
            vm.RecentApplications = new List<RecentApplication>();
        }

        return View(vm);
    }
}
