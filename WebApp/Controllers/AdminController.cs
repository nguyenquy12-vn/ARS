using Domain.Enums;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Controllers;

[Route("admin")]
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ARSDbContext _context;
    public AdminController(ARSDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        var today = DateTime.UtcNow.Date;
        ViewBag.TotalUsers = await _context.Users.CountAsync();
        ViewBag.Candidates = await _context.Users.CountAsync(x => x.Role != null && x.Role.Name == "Candidate");
        ViewBag.Recruiters = await _context.Users.CountAsync(x => x.Role != null && x.Role.Name == "Recruiter");
        ViewBag.ActiveJobs = await _context.JobPostings.CountAsync(x => x.Status == JobStatus.Active);
        ViewBag.Applications = await _context.Applications.CountAsync();
        ViewBag.TodayApplications = await _context.Applications.CountAsync(x => x.AppliedAt >= today);
        ViewBag.Revenue = await _context.PaymentOrders.Where(x => x.Status == PaymentStatus.Successful).SumAsync(x => (decimal?)x.Amount) ?? 0;
        ViewBag.PendingPayments = await _context.PaymentOrders.CountAsync(x => x.Status == PaymentStatus.PendingConfirmation);
        ViewBag.RecentApplications = await _context.Applications.Include(x => x.Candidate).Include(x => x.JobPosting).OrderByDescending(x => x.AppliedAt).Take(6).ToListAsync();
        ViewBag.RecentUsers = await _context.Users.Include(x => x.Role).OrderByDescending(x => x.CreatedAt).Take(5).ToListAsync();
        return View();
    }
}
