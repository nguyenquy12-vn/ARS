using System.Security.Claims;
using Domain.Enums;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Filters;

public sealed class RequireActiveRecruiterPlanAttribute : TypeFilterAttribute
{
    public RequireActiveRecruiterPlanAttribute() : base(typeof(RequireActiveRecruiterPlanFilter)) { }
}

public sealed class RequireActiveRecruiterPlanFilter : IAsyncActionFilter
{
    private readonly ARSDbContext _context;

    public RequireActiveRecruiterPlanFilter(ARSDbContext context) => _context = context;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var idValue = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idValue, out var recruiterId))
        {
            context.Result = new ChallengeResult();
            return;
        }

        // Notification endpoints must stay usable so recruiters do not miss new CVs.
        var controllerName = context.RouteData.Values["controller"]?.ToString();
        var actionName = context.RouteData.Values["action"]?.ToString();
        if (string.Equals(controllerName, "Application", StringComparison.OrdinalIgnoreCase) &&
            actionName is "Notifications" or "GetUnreadCount" or "MarkRead" or "MarkAllRead")
        {
            await next();
            return;
        }

        var hasActivePlan = await _context.PaymentOrders
            .AnyAsync(order => order.RecruiterId == recruiterId && order.Status == PaymentStatus.Successful);
        if (hasActivePlan)
        {
            await next();
            return;
        }

        if (context.HttpContext.Request.Headers.XRequestedWith == "XMLHttpRequest")
        {
            context.Result = new JsonResult(new { message = "Bạn cần có gói dịch vụ đã được Admin duyệt." }) { StatusCode = StatusCodes.Status403Forbidden };
            return;
        }

        if (context.Controller is Controller controller)
            controller.TempData["Error"] = "Tính năng này bị khóa cho đến khi Admin duyệt đơn thanh toán của bạn.";
        context.Result = new RedirectToActionResult("Index", "Billing", null);
    }
}
