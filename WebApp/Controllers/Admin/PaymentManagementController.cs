using Domain.Enums;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Controllers.Admin;

[Authorize(Roles = "Admin")]
public class PaymentManagementController : Controller
{
    private readonly ARSDbContext _context;

    public PaymentManagementController(ARSDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var orders = await _context.PaymentOrders
            .Include(x => x.Recruiter)
            .OrderBy(x => x.Status == PaymentStatus.PendingConfirmation ? 0 : 1)
            .ThenByDescending(x => x.CreatedAt)
            .ToListAsync();
        var successful = orders.Where(x => x.Status == PaymentStatus.Successful).ToList();
        var today = DateTime.Today;
        ViewBag.TotalRevenue = successful.Sum(x => x.Amount);
        ViewBag.MonthRevenue = successful.Where(x => x.ReviewedAt?.ToLocalTime().Year == today.Year && x.ReviewedAt?.ToLocalTime().Month == today.Month).Sum(x => x.Amount);
        ViewBag.TodayRevenue = successful.Where(x => x.ReviewedAt?.ToLocalTime().Date == today).Sum(x => x.Amount);
        ViewBag.SuccessCount = successful.Count;
        ViewBag.StarterSales = successful.Count(x => x.PlanCode == "Starter");
        ViewBag.ProSales = successful.Count(x => x.PlanCode == "Pro");
        ViewBag.DailyRevenue = Enumerable.Range(0, 7)
            .Select(offset => today.AddDays(offset - 6))
            .Select(day => new RevenuePoint(day.ToString("dd/MM"), successful
                .Where(x => x.ReviewedAt?.ToLocalTime().Date == day)
                .Sum(x => x.Amount)))
            .ToList();
        return View(orders);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Review(int id, PaymentStatus status, string? adminNote)
    {
        if (status is not (PaymentStatus.Successful or PaymentStatus.Failed))
        {
            TempData["Error"] = "Admin chỉ có thể duyệt thành công hoặc thất bại.";
            return RedirectToAction(nameof(Index));
        }

        var order = await _context.PaymentOrders.FindAsync(id);
        if (order is null) return NotFound();

        if (order.Status != PaymentStatus.PendingConfirmation)
        {
            TempData["Error"] = "Đơn này không còn chờ xác nhận.";
            return RedirectToAction(nameof(Index));
        }

        if (status == PaymentStatus.Successful)
        {
            var hasAnotherPendingOrder = await _context.PaymentOrders
                .AnyAsync(x => x.RecruiterId == order.RecruiterId && x.Id != order.Id && x.Status == PaymentStatus.PendingConfirmation);
            if (hasAnotherPendingOrder)
            {
                TempData["Error"] = "Recruiter còn một đơn chờ khác. Hãy yêu cầu họ hủy đơn không dùng trước khi duyệt.";
                return RedirectToAction(nameof(Index));
            }

            var currentPlan = await _context.PaymentOrders
                .Where(x => x.RecruiterId == order.RecruiterId && x.Status == PaymentStatus.Successful)
                .OrderByDescending(x => x.ReviewedAt ?? x.CreatedAt)
                .Select(x => x.PlanCode)
                .FirstOrDefaultAsync();
            if (currentPlan == "Pro" || (currentPlan == "Starter" && order.PlanCode != "Pro"))
            {
                TempData["Error"] = "Đơn không phù hợp với gói hiện tại của recruiter nên không thể duyệt thành công.";
                return RedirectToAction(nameof(Index));
            }
        }

        order.Status = status;
        order.AdminNote = adminNote?.Trim();
        order.ReviewedAt = DateTime.UtcNow;
        order.ReviewedByUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật trạng thái đơn thanh toán.";
        return RedirectToAction(nameof(Index));
    }

    public sealed record RevenuePoint(string Label, decimal Amount);
}
