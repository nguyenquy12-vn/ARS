using System.Security.Claims;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Filters;

// [BẢO VỆ] FILTER PRO: khóa Kho CV/báo cáo với Free và Starter.
public sealed class RequireProPlanAttribute : TypeFilterAttribute
{
    public RequireProPlanAttribute() : base(typeof(RequireProPlanFilter)) { }
}

public sealed class RequireProPlanFilter : IAsyncActionFilter
{
    private readonly ARSDbContext _context;
    public RequireProPlanFilter(ARSDbContext context) => _context = context;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var idValue = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idValue, out var recruiterId))
        {
            context.Result = new ChallengeResult();
            return;
        }

        var isPro = await _context.RecruiterSubscriptions.AnyAsync(x =>
            x.RecruiterId == recruiterId && x.PlanCode == "Pro" && x.ExpiresAt > DateTime.UtcNow);
        if (isPro)
        {
            await next();
            return;
        }

        const string message = "Tính năng này chỉ dành cho gói Pro. Vui lòng nâng cấp để sử dụng AI, Kho CV và báo cáo tuyển dụng.";
        if (context.HttpContext.Request.Headers.XRequestedWith == "XMLHttpRequest")
        {
            context.Result = new JsonResult(new { ok = false, error = message }) { StatusCode = StatusCodes.Status403Forbidden };
            return;
        }
        if (context.Controller is Controller controller) controller.TempData["Error"] = message;
        context.Result = new RedirectToActionResult("Index", "Billing", null);
    }
}
