using System.Security.Claims;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Filters;

// [BẢO VỆ] FILTER AI: chỉ gói Free dùng thử hoặc Pro còn hạn được gọi AI.
public sealed class RequireAiPlanAttribute : TypeFilterAttribute
{
    public RequireAiPlanAttribute() : base(typeof(RequireAiPlanFilter)) { }
}

public sealed class RequireAiPlanFilter : IAsyncActionFilter
{
    private readonly ARSDbContext _context;
    public RequireAiPlanFilter(ARSDbContext context) => _context = context;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var idValue = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idValue, out var recruiterId))
        {
            context.Result = new ChallengeResult();
            return;
        }

        var hasAi = await _context.RecruiterSubscriptions.AnyAsync(x =>
            x.RecruiterId == recruiterId && (x.PlanCode == "Pro" || x.PlanCode == "Free") && x.ExpiresAt > DateTime.UtcNow);
        if (hasAi)
        {
            await next();
            return;
        }

        const string message = "Tính năng AI chỉ dành cho gói Free dùng thử hoặc Pro.";
        if (context.HttpContext.Request.Headers.XRequestedWith == "XMLHttpRequest")
        {
            context.Result = new JsonResult(new { ok = false, error = message }) { StatusCode = StatusCodes.Status403Forbidden };
            return;
        }
        if (context.Controller is Controller controller) controller.TempData["Error"] = message;
        context.Result = new RedirectToActionResult("Index", "Billing", null);
    }
}
