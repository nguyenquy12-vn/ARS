using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Services.Interfaces; // Namespace chứa IUserService hoặc DbContext

namespace WebApp.Middlewares;

public class UserStatusValidationMiddleware
{
    private readonly RequestDelegate _next;

    public UserStatusValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IUserService userService)
    {
        if (context.User.Identity != null && context.User.Identity.IsAuthenticated)
        {
            var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdClaim, out int userId))
            {
                bool isLocked = await userService.IsUserLockedAsync(userId);

                if (isLocked)
                {
                    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                    context.Response.Redirect("login?reason=locked");
                    return;
                }


            }
        }

        await _next(context);
    }
}