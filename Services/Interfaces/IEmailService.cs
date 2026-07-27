using Domain.Entities;

namespace Services.Interfaces;

public interface IEmailService
{
    // Gửi email dùng cấu hình SMTP của chính recruiter (sender). Trả về (ok, lỗi).
    Task<(bool ok, string? error)> SendAsync(User sender, string toEmail, string subject, string htmlBody);
}
