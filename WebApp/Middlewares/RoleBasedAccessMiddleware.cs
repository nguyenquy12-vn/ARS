using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace WebApp.Middlewares;

// [BẢO VỆ] PHÂN LUỒNG THEO VAI TRÒ: mỗi role chỉ vào được khu vực chức năng của mình.
// Khi một role điều hướng (GET) sang khu vực không thuộc quyền của họ, tự chuyển hướng về
// trang chủ tương ứng thay vì hiện trang từ chối truy cập.
//   - Admin      -> /admin
//   - Recruiter  -> /JobPosting
//   - Candidate  -> /  (trang chủ)
//
// Middleware KHÔNG dùng danh sách controller cứng cho từng role. Thay vào đó nó ĐỌC chính quyền
// khai báo trên endpoint ([Authorize(Roles=...)] / [AllowAnonymous]) để quyết định:
//   - Endpoint [AllowAnonymous]            -> luôn cho qua (vd VnPay/Return, đăng nhập).
//   - Endpoint có [Authorize(Roles=...)]   -> nếu role user nằm trong danh sách thì CHO QUA,
//                                             kể cả khi controller đó không thuộc "khu vực gốc"
//                                             của họ (giải quyết nhu cầu gọi chéo controller).
//   - Endpoint công khai / theo policy      -> coi là khu vực Candidate/khách; recruiter & admin
//                                             bị chuyển về trang chủ của họ.
public class RoleBasedAccessMiddleware
{
    private readonly RequestDelegate _next;

    // Controller dùng chung cho mọi vai trò (đăng nhập/đăng xuất, đổi mật khẩu...) - không chặn.
    private static readonly HashSet<string> SharedControllers = new(StringComparer.OrdinalIgnoreCase)
    {
        "Auth"
    };

    // Khu vực dành cho Candidate / khách vãng lai (trang công khai, không giới hạn theo role
    // hoặc chỉ giới hạn bằng policy). Recruiter & Admin khi vào đây sẽ bị đưa về trang chủ của họ.
    private static readonly HashSet<string> CandidateAreaControllers = new(StringComparer.OrdinalIgnoreCase)
    {
        "Home", "Job", "Candidate", "RecruiterRequest"
    };

    public RoleBasedAccessMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var user = context.User;

        // Chỉ áp dụng cho request điều hướng (GET) của người dùng đã đăng nhập.
        if (user.Identity?.IsAuthenticated == true && HttpMethods.IsGet(context.Request.Method))
        {
            var endpoint = context.GetEndpoint();

            // Bỏ qua static file, SignalR hub và endpoint cho phép ẩn danh (vd VnPay/Return).
            if (endpoint != null && endpoint.Metadata.GetMetadata<IAllowAnonymous>() == null)
            {
                var controller = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>()?.ControllerName;

                if (!string.IsNullOrEmpty(controller) && !SharedControllers.Contains(controller))
                {
                    var home = HomeForUser(user);

                    if (home != null)
                    {
                        var allowedRoles = GetAllowedRoles(endpoint);
                        bool allowed;

                        if (allowedRoles.Count > 0)
                        {
                            // Endpoint giới hạn theo role cụ thể -> cho qua nếu user thuộc role đó
                            // (kể cả controller nằm ngoài khu vực gốc của họ).
                            allowed = allowedRoles.Any(user.IsInRole);
                        }
                        else
                        {
                            // Không giới hạn theo role (công khai / theo policy):
                            // coi là khu vực Candidate -> chỉ Candidate được vào.
                            allowed = !CandidateAreaControllers.Contains(controller) || user.IsInRole("Candidate");
                        }

                        if (!allowed)
                        {
                            context.Response.Redirect(home);
                            return;
                        }
                    }
                }
            }
        }

        await _next(context);
    }

    // Trang chủ tương ứng với vai trò của người dùng.
    private static string? HomeForUser(System.Security.Claims.ClaimsPrincipal user)
    {
        if (user.IsInRole("Admin")) return "/admin";
        if (user.IsInRole("Recruiter")) return "/JobPosting";
        if (user.IsInRole("Candidate")) return "/";
        return null;
    }

    // Gom toàn bộ role được phép từ các [Authorize(Roles=...)] trên endpoint (controller + action).
    private static HashSet<string> GetAllowedRoles(Endpoint endpoint)
    {
        var roles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var attr in endpoint.Metadata.GetOrderedMetadata<AuthorizeAttribute>())
        {
            if (string.IsNullOrWhiteSpace(attr.Roles))
            {
                continue;
            }

            foreach (var role in attr.Roles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                roles.Add(role);
            }
        }

        return roles;
    }
}

