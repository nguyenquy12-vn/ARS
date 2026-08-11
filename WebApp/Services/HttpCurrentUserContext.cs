using System.Security.Claims;
using Services.Interfaces;

namespace WebApp.Support;

// Lấy Id user đang đăng nhập từ claim (NameIdentifier) để tầng Services dùng.
public class HttpCurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _accessor;

    public HttpCurrentUserContext(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public int? GetCurrentUserId()
    {
        var value = _accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id) ? id : null;
    }
}
