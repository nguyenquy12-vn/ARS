using Domain.Enums;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Controllers.Admin;

// [BẢO VỆ] ADMIN ĐƠN ỨNG TUYỂN: tìm kiếm/lọc toàn bộ Application và Include quan hệ.
[Authorize(Roles = "Admin")]
[Route("admin/applications")]
public class ApplicationManagementController : Controller
{
    private readonly ARSDbContext _context;
    public ApplicationManagementController(ARSDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> Index(string? keyword, ApplicationStatus? status)
    {
        var query = _context.Applications.Include(x => x.Candidate).Include(x => x.JobPosting).ThenInclude(x => x!.Company).AsQueryable();
        if (!string.IsNullOrWhiteSpace(keyword)) query = query.Where(x => x.Candidate!.FullName.Contains(keyword) || x.Candidate.Email.Contains(keyword) || x.JobPosting!.Title.Contains(keyword));
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        ViewBag.Keyword = keyword;
        ViewBag.Status = status;
        ViewBag.Total = await _context.Applications.CountAsync();
        ViewBag.Accepted = await _context.Applications.CountAsync(x => x.Status == ApplicationStatus.Accepted);
        ViewBag.Rejected = await _context.Applications.CountAsync(x => x.Status == ApplicationStatus.Rejected);
        return View(await query.OrderByDescending(x => x.AppliedAt).ToListAsync());
    }
}
