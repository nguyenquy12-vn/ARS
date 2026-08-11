namespace Services.Interfaces;

// Cho phép tầng Services biết user đang đăng nhập là ai (cài đặt ở WebApp qua HttpContext).
public interface ICurrentUserContext
{
    int? GetCurrentUserId();
}
