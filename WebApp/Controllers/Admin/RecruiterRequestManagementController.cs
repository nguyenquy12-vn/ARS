using Domain.Entities;
using Domain.Enums;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("admin/recruiter-requests")]
public class RecruiterRequestManagementController : Controller
{
    private readonly ARSDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public RecruiterRequestManagementController(ARSDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(RecruiterRequestStatus? status)
    {
        var query = _context.RecruiterRequests.Include(x => x.User).AsNoTracking();
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        ViewBag.Status = status;
        return View(await query.OrderBy(x => x.Status != RecruiterRequestStatus.Pending)
            .ThenByDescending(x => x.CreatedAt).ToListAsync());
    }

    [HttpGet("document/{id:int}")]
    public async Task<IActionResult> Document(int id)
    {
        var request = await _context.RecruiterRequests.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (request == null) return NotFound();
        var path = Path.Combine(_environment.ContentRootPath, "App_Data", "recruiter-requests", Path.GetFileName(request.DocumentPath));
        if (!System.IO.File.Exists(path)) return NotFound();
        var extension = Path.GetExtension(path).ToLowerInvariant();
        var contentType = extension == ".pdf" ? "application/pdf" : extension is ".jpg" or ".jpeg" ? "image/jpeg" : "image/png";
        return PhysicalFile(path, contentType);
    }

    [HttpPost("approve/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id, string? adminNotes)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        var request = await _context.RecruiterRequests.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == id);
        if (request == null || request.Status != RecruiterRequestStatus.Pending)
            return RedirectWithError("Yêu cầu không tồn tại hoặc đã được xử lý.");

        var recruiterRole = await _context.Roles.FirstOrDefaultAsync(x => x.Name == "Recruiter");
        if (request.User == null || recruiterRole == null)
            return RedirectWithError("Không tìm thấy tài khoản hoặc vai trò Recruiter.");

        var duplicateTaxCode = await _context.Companies.AnyAsync(x => x.TaxCode == request.TaxCode && x.RecruiterId != request.UserId);
        if (duplicateTaxCode) return RedirectWithError("Mã số thuế này đã được một công ty khác sử dụng.");

        request.User.RoleId = recruiterRole.Id;
        request.User.Role = recruiterRole;
        request.Status = RecruiterRequestStatus.Approved;
        request.AdminNotes = string.IsNullOrWhiteSpace(adminNotes) ? "Hồ sơ hợp lệ." : adminNotes.Trim();
        request.ProcessedAt = DateTime.UtcNow;

        if (!await _context.Companies.AnyAsync(x => x.RecruiterId == request.UserId))
        {
            _context.Companies.Add(new Company
            {
                RecruiterId = request.UserId,
                CompanyName = request.CompanyName,
                TaxCode = request.TaxCode
            });
        }

        AddNotification(request, true);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        TempData["Success"] = $"Đã duyệt {request.User.FullName}. Người dùng cần đăng nhập lại để nhận quyền Recruiter.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("reject/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string adminNotes)
    {
        var request = await _context.RecruiterRequests.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == id);
        if (request == null || request.Status != RecruiterRequestStatus.Pending)
            return RedirectWithError("Yêu cầu không tồn tại hoặc đã được xử lý.");
        if (string.IsNullOrWhiteSpace(adminNotes))
            return RedirectWithError("Vui lòng nhập lý do từ chối.");

        request.Status = RecruiterRequestStatus.Rejected;
        request.AdminNotes = adminNotes.Trim();
        request.ProcessedAt = DateTime.UtcNow;
        AddNotification(request, false);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Đã từ chối yêu cầu và gửi lý do cho người dùng.";
        return RedirectToAction(nameof(Index));
    }

    private void AddNotification(RecruiterRequest request, bool approved)
    {
        _context.Notifications.Add(new Notification
        {
            UserId = request.UserId,
            Title = approved ? "Yêu cầu Recruiter đã được duyệt" : "Yêu cầu Recruiter bị từ chối",
            Message = approved
                ? "Hồ sơ đã được duyệt. Hãy đăng xuất và đăng nhập lại để sử dụng quyền Recruiter."
                : $"Lý do: {request.AdminNotes}",
            Type = approved ? "RecruiterApproved" : "RecruiterRejected",
            RelatedId = request.Id,
            CreatedAt = DateTime.UtcNow
        });
    }

    private IActionResult RedirectWithError(string message)
    {
        TempData["Error"] = message;
        return RedirectToAction(nameof(Index));
    }
}
