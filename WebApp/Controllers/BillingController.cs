using System.Security.Claims;
using Domain.Entities;
using Domain.Enums;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Controllers;

[Authorize(Roles = "Recruiter")]
public class BillingController : Controller
{
    private readonly ARSDbContext _context;
    private readonly IConfiguration _configuration;

    public BillingController(ARSDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var orders = await _context.PaymentOrders
            .Where(x => x.RecruiterId == CurrentUserId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
        ViewBag.CurrentPlan = orders
            .Where(x => x.Status == PaymentStatus.Successful)
            .OrderByDescending(x => x.ReviewedAt ?? x.CreatedAt)
            .Select(x => x.PlanCode)
            .FirstOrDefault();
        return View(orders);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateOrder(string planCode)
    {
        var hasPendingOrder = await _context.PaymentOrders
            .AnyAsync(x => x.RecruiterId == CurrentUserId && x.Status == PaymentStatus.PendingConfirmation);
        if (hasPendingOrder)
        {
            TempData["Error"] = "Bạn đang có đơn chờ xác nhận. Hãy tiếp tục thanh toán hoặc hủy đơn đó trước khi chọn gói khác.";
            return RedirectToAction(nameof(Index));
        }

        var plan = planCode switch
        {
            "Starter" => (Name: "Starter", Amount: 690_000m),
            "Pro" => (Name: "Pro", Amount: 1_490_000m),
            _ => ((string Name, decimal Amount)?)null
        };

        if (plan is null)
        {
            TempData["Error"] = "Gói thanh toán không hợp lệ.";
            return RedirectToAction(nameof(Index));
        }

        var currentPlan = await _context.PaymentOrders
            .Where(x => x.RecruiterId == CurrentUserId && x.Status == PaymentStatus.Successful)
            .OrderByDescending(x => x.ReviewedAt ?? x.CreatedAt)
            .Select(x => x.PlanCode)
            .FirstOrDefaultAsync();

        if (currentPlan == "Pro")
        {
            TempData["Error"] = "Tài khoản của bạn đang ở gói Pro, không có gói nâng cấp cao hơn.";
            return RedirectToAction(nameof(Index));
        }

        if (currentPlan == "Starter" && planCode != "Pro")
        {
            TempData["Error"] = "Tài khoản Starter chỉ có thể mua gói nâng cấp Pro.";
            return RedirectToAction(nameof(Index));
        }

        var order = new PaymentOrder
        {
            RecruiterId = CurrentUserId,
            PlanCode = planCode,
            PlanName = plan.Value.Name,
            Amount = plan.Value.Amount,
            TransferCode = $"ARS{DateTime.UtcNow:yyMMdd}{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}"
        };

        _context.PaymentOrders.Add(order);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Checkout), new { id = order.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelOrder(int id)
    {
        var order = await _context.PaymentOrders.FirstOrDefaultAsync(x => x.Id == id && x.RecruiterId == CurrentUserId);
        if (order is null) return NotFound();
        if (order.Status != PaymentStatus.PendingConfirmation)
        {
            TempData["Error"] = "Chỉ có thể hủy đơn đang chờ xác nhận.";
            return RedirectToAction(nameof(Index));
        }

        order.Status = PaymentStatus.Cancelled;
        order.AdminNote = "Recruiter đã hủy đơn thanh toán này.";
        await _context.SaveChangesAsync();
        TempData["Success"] = "Đã hủy đơn. Bạn có thể chọn gói thanh toán khác.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmQrPayment(int id)
    {
        var order = await _context.PaymentOrders.FirstOrDefaultAsync(x => x.Id == id && x.RecruiterId == CurrentUserId);
        if (order is null) return NotFound();
        if (order.Status != PaymentStatus.PendingConfirmation)
        {
            TempData["Error"] = "Đơn này không còn chờ xác nhận.";
            return RedirectToAction(nameof(Index));
        }

        order.AdminNote = "Recruiter đã xác nhận thanh toán bằng VietQR. Chờ Admin đối chiếu và duyệt.";
        await _context.SaveChangesAsync();
        TempData["Success"] = "Đã ghi nhận thanh toán QR. Đơn đang chờ Admin duyệt.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Checkout(int id)
    {
        var order = await _context.PaymentOrders.FirstOrDefaultAsync(x => x.Id == id && x.RecruiterId == CurrentUserId);
        if (order is null) return NotFound();
        if (order.Status != PaymentStatus.PendingConfirmation)
        {
            TempData["Error"] = "Đơn này không còn ở trạng thái chờ thanh toán.";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.BankId = _configuration["Payments:BankId"] ?? "MB";
        ViewBag.AccountNumber = _configuration["Payments:AccountNumber"] ?? "CHUA_CAU_HINH";
        ViewBag.AccountName = _configuration["Payments:AccountName"] ?? "ARS RECRUITMENT";
        return View(order);
    }
}
