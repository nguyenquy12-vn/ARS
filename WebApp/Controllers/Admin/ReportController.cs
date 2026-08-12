using Domain.Enums;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Models.Admin;

namespace WebApp.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("admin/reports")]
public class ReportController : Controller
{
    private readonly ARSDbContext _context;
    public ReportController(ARSDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var applications = await _context.Applications.AsNoTracking().ToListAsync();
        var users = await _context.Users.AsNoTracking().Include(x => x.Role).ToListAsync();
        var jobs = await _context.JobPostings.AsNoTracking()
            .Include(x => x.JobCategory)
            .Include(x => x.Company)
            .Include(x => x.Applications)
            .ToListAsync();
        var now = DateTime.UtcNow;
        var months = Enumerable.Range(0, 6).Select(i => new DateTime(now.Year, now.Month, 1).AddMonths(i - 5)).ToList();
        var successful = applications.Count(x => x.Status is ApplicationStatus.Accepted or ApplicationStatus.Interview);

        var model = new AdminReportViewModel
        {
            TotalUsers = users.Count,
            TotalJobs = jobs.Count,
            ActiveJobs = jobs.Count(x => x.Status == JobStatus.Active),
            TotalApplications = applications.Count,
            SuccessfulApplications = successful,
            TotalRevenue = await _context.PaymentOrders.Where(x => x.Status == PaymentStatus.Successful).SumAsync(x => (decimal?)x.Amount) ?? 0,
            ConversionRate = applications.Count == 0 ? 0 : Math.Round(successful * 100d / applications.Count, 1),
            AverageAiScore = applications.Any(x => x.AiMatchScore.HasValue) ? Math.Round(applications.Where(x => x.AiMatchScore.HasValue).Average(x => x.AiMatchScore!.Value), 1) : 0,
            UsersByRole = users.GroupBy(x => x.Role?.DisplayedName ?? "Chưa phân vai trò").Select(x => new ReportPoint(x.Key, x.Count())).OrderByDescending(x => x.Value).ToList(),
            JobsByCategory = jobs.GroupBy(x => x.JobCategory?.Name ?? "Chưa phân loại").Select(x => new ReportPoint(x.Key, x.Count())).OrderByDescending(x => x.Value).ToList(),
            ApplicationsByStatus = applications.GroupBy(x => StatusLabel(x.Status)).Select(x => new ReportPoint(x.Key, x.Count())).OrderByDescending(x => x.Value).ToList(),
            MonthlyApplications = months.Select(m => new ReportPoint(m.ToString("MM/yyyy"), applications.Count(x => x.AppliedAt.Year == m.Year && x.AppliedAt.Month == m.Month))).ToList(),
            MonthlyUsers = months.Select(m => new ReportPoint(m.ToString("MM/yyyy"), users.Count(x => x.CreatedAt.Year == m.Year && x.CreatedAt.Month == m.Month))).ToList(),
            TopJobs = jobs.OrderByDescending(x => x.Applications.Count).Take(8)
                .Select(x => new TopJobReport(x.Title, x.Company?.CompanyName ?? "Chưa cập nhật", x.Applications.Count,
                    x.Applications.Any(a => a.AiMatchScore.HasValue) ? x.Applications.Where(a => a.AiMatchScore.HasValue).Average(a => a.AiMatchScore!.Value) : 0,
                    x.Applications.Count(a => a.Status == ApplicationStatus.Accepted || a.Status == ApplicationStatus.Interview)))
                .ToList()
        };
        return View(model);
    }

    private static string StatusLabel(ApplicationStatus status) => status switch
    {
        ApplicationStatus.Pending => "Chờ xem",
        ApplicationStatus.Reviewing => "Đang xem xét",
        ApplicationStatus.EvaluatingAI => "AI đang đánh giá",
        ApplicationStatus.Accepted => "Đã chấp nhận",
        ApplicationStatus.Rejected => "Đã từ chối",
        ApplicationStatus.Interview => "Đã hẹn phỏng vấn",
        ApplicationStatus.Withdrawn => "Đã rút đơn",
        _ => status.ToString()
    };
}
