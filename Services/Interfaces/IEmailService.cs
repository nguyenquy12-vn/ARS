<<<<<<< HEAD
=======
using Domain.Entities;

>>>>>>> origin/features
namespace Services.Interfaces;

public interface IEmailService
{
<<<<<<< HEAD
    Task SendEmailAsync(string to, string subject, string htmlBody);
=======
    // Gửi email dùng cấu hình SMTP của chính recruiter (sender). Trả về (ok, lỗi).
    Task<(bool ok, string? error)> SendAsync(User sender, string toEmail, string subject, string htmlBody);
>>>>>>> origin/features
}
