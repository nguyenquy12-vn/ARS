using Domain.Entities;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Services.Interfaces;

namespace Services.Implementations;

// Gửi email qua SMTP dùng cấu hình lưu trong hồ sơ cá nhân của recruiter.
// Dùng MailKit để hỗ trợ cả SSL ngầm định (port 465) lẫn STARTTLS (port 587).
public class EmailService : IEmailService
{
    public async Task<(bool ok, string? error)> SendAsync(User sender, string toEmail, string subject, string htmlBody)
    {
        if (string.IsNullOrWhiteSpace(sender.SmtpHost) || sender.SmtpPort is null or 0
            || string.IsNullOrWhiteSpace(sender.SmtpUsername) || string.IsNullOrWhiteSpace(sender.SmtpPassword))
        {
            return (false, "Bạn chưa cấu hình email trong Hồ sơ cá nhân.");
        }

        if (string.IsNullOrWhiteSpace(toEmail))
        {
            return (false, "Ứng viên không có email hợp lệ.");
        }

        try
        {
            var fromEmail = string.IsNullOrWhiteSpace(sender.SmtpFromEmail) ? sender.SmtpUsername! : sender.SmtpFromEmail!;
            var port = sender.SmtpPort!.Value;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(sender.FullName, fromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

            // Port 465 = SSL ngầm định; 587 (hoặc bật SSL) = STARTTLS; còn lại = không mã hoá
            SecureSocketOptions secure = port == 465
                ? SecureSocketOptions.SslOnConnect
                : sender.SmtpEnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;

            using var client = new SmtpClient();
            await client.ConnectAsync(sender.SmtpHost, port, secure);
            await client.AuthenticateAsync(sender.SmtpUsername, sender.SmtpPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, "Gửi email thất bại: " + ex.Message);
        }
    }
}
