using System.Security.Claims;
using Domain.Entities;
using Domain.Enums;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Models.RecruiterRequest;

namespace WebApp.Controllers;

[Authorize(Roles = "Candidate")]
[Route("recruiter-request")]
public class RecruiterRequestController : Controller
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".jpg", ".jpeg", ".png"
    };

    private readonly ARSDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public RecruiterRequestController(ARSDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        ViewBag.LatestRequest = await _context.RecruiterRequests
            .AsNoTracking()
            .Where(x => x.UserId == CurrentUserId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();
        return View(new CreateRecruiterRequestViewModel());
    }

    [HttpPost("")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> Create(CreateRecruiterRequestViewModel model)
    {
        var hasPending = await _context.RecruiterRequests
            .AnyAsync(x => x.UserId == CurrentUserId && x.Status == RecruiterRequestStatus.Pending);
        if (hasPending)
            ModelState.AddModelError(string.Empty, "Bạn đang có một yêu cầu chờ Admin xử lý.");

        if (model.Document is not null)
        {
            var extension = Path.GetExtension(model.Document.FileName);
            if (!AllowedExtensions.Contains(extension))
                ModelState.AddModelError(nameof(model.Document), "Chỉ chấp nhận PDF, JPG, JPEG hoặc PNG.");
            if (model.Document.Length > 5 * 1024 * 1024)
                ModelState.AddModelError(nameof(model.Document), "Giấy tờ không được vượt quá 5MB.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.LatestRequest = await _context.RecruiterRequests.AsNoTracking()
                .Where(x => x.UserId == CurrentUserId).OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync();
            return View("Index", model);
        }

        var extensionName = Path.GetExtension(model.Document!.FileName).ToLowerInvariant();
        var storedName = $"{Guid.NewGuid():N}{extensionName}";
        var directory = Path.Combine(_environment.ContentRootPath, "App_Data", "recruiter-requests");
        Directory.CreateDirectory(directory);
        await using (var stream = System.IO.File.Create(Path.Combine(directory, storedName)))
            await model.Document.CopyToAsync(stream);

        _context.RecruiterRequests.Add(new RecruiterRequest
        {
            UserId = CurrentUserId,
            CompanyName = model.CompanyName.Trim(),
            TaxCode = model.TaxCode.Trim(),
            DocumentPath = storedName,
            Status = RecruiterRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        TempData["Success"] = "Đã gửi yêu cầu. Admin sẽ kiểm tra và phản hồi cho bạn.";
        return RedirectToAction(nameof(Index));
    }
}
