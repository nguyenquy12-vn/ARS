using System.Security.Claims;
using Domain.Entities;
using Domain.Enums;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Controllers;

// [BẢO VỆ] GÓI DỊCH VỤ: PaymentOrder là lịch sử; RecruiterSubscription là gói hiện tại + hạn.
// Free dùng thử một lần/24 giờ; Starter và Pro tạo đơn rồi thanh toán qua VNPAY.
[Authorize(Roles = "Recruiter")]
public class BillingController : Controller
{
    private readonly ARSDbContext _context;

    public BillingController(ARSDbContext context)
    {
        _context = context;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var orders = await _context.PaymentOrders
            .Where(x => x.RecruiterId == CurrentUserId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
        var subscription = await _context.RecruiterSubscriptions
            .FirstOrDefaultAsync(x => x.RecruiterId == CurrentUserId);

        // Đồng bộ các gói đã bị Admin hủy trước khi lịch sử thanh toán hỗ trợ trạng thái này.
        if (subscription is not null && subscription.ExpiresAt <= DateTime.UtcNow &&
            subscription.AdminNote?.Contains("Admin", StringComparison.OrdinalIgnoreCase) == true &&
            subscription.AdminNote.Contains("hủy", StringComparison.OrdinalIgnoreCase))
        {
            var legacyOrder = orders.FirstOrDefault(x =>
                x.PlanCode == subscription.PlanCode && x.Status == PaymentStatus.Successful);
            if (legacyOrder is not null)
            {
                legacyOrder.Status = PaymentStatus.Cancelled;
                legacyOrder.AdminNote = subscription.AdminNote;
                await _context.SaveChangesAsync();
            }
        }

        var isActive = subscription?.ExpiresAt > DateTime.UtcNow;
        ViewBag.CurrentPlan = isActive ? subscription!.PlanCode : null;
        ViewBag.PlanExpiresAt = isActive ? subscription!.ExpiresAt : (DateTime?)null;
        ViewBag.HasUsedFreeTrial = await _context.PaymentOrders
            .AnyAsync(x => x.RecruiterId == CurrentUserId && x.PlanCode == "Free");
        return View(orders);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // [BẢO VỆ] KÍCH HOẠT FREE: chống dùng lần hai bằng PaymentOrder PlanCode="Free", hạn đúng 24 giờ.
    public async Task<IActionResult> ActivateFreeTrial()
    {
        var now = DateTime.UtcNow;
        var hasUsedTrial = await _context.PaymentOrders
            .AnyAsync(x => x.RecruiterId == CurrentUserId && x.PlanCode == "Free");
        if (hasUsedTrial)
        {
            TempData["Error"] = "Mỗi tài khoản chỉ được sử dụng gói Free một lần.";
            return RedirectToAction(nameof(Index));
        }

        var subscription = await _context.RecruiterSubscriptions
            .FirstOrDefaultAsync(x => x.RecruiterId == CurrentUserId);
        if (subscription?.ExpiresAt > now)
        {
            TempData["Error"] = "Tài khoản đang có một gói hoạt động nên không thể kích hoạt dùng thử.";
            return RedirectToAction(nameof(Index));
        }

        var trialOrder = new PaymentOrder
        {
            RecruiterId = CurrentUserId,
            PlanCode = "Free",
            PlanName = "Free dùng thử 1 ngày",
            Amount = 0,
            TransferCode = $"TRIAL{DateTime.UtcNow:yyMMdd}{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}",
            Status = PaymentStatus.Successful,
            ReviewedAt = now,
            AdminNote = "Kích hoạt gói Free dùng thử tự động."
        };
        _context.PaymentOrders.Add(trialOrder);

        if (subscription is null)
        {
            subscription = new RecruiterSubscription { RecruiterId = CurrentUserId };
            _context.RecruiterSubscriptions.Add(subscription);
        }
        subscription.PlanCode = "Free";
        subscription.StartedAt = now;
        subscription.ExpiresAt = now.AddDays(1);
        subscription.UpdatedAt = now;
        subscription.AdminNote = "Gói Free dùng thử 1 ngày.";
        await _context.SaveChangesAsync();

        TempData["Success"] = "Đã kích hoạt gói Free. Bạn có 24 giờ dùng thử và được đăng tối đa 1 bài tuyển dụng.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // [BẢO VỆ] TẠO ĐƠN STARTER/PRO: chưa thu tiền ở đây; chỉ tạo PaymentOrder chờ VNPAY.
    public async Task<IActionResult> CreateOrder(string planCode)
    {
        var hasPendingOrder = await _context.PaymentOrders
            .AnyAsync(x => x.RecruiterId == CurrentUserId && x.Status == PaymentStatus.PendingConfirmation);
        if (hasPendingOrder)
        {
            TempData["Error"] = "Bạn đang có đơn chưa thanh toán. Hãy tiếp tục qua VNPAY hoặc hủy đơn đó trước khi chọn gói khác.";
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

        var currentPlan = await _context.RecruiterSubscriptions
            .Where(x => x.RecruiterId == CurrentUserId && x.ExpiresAt > DateTime.UtcNow)
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

        return View(order);
    }
}
