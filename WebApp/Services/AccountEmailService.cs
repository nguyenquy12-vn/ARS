using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace WebApp.Accounts;

public interface IAccountEmailService
{
    Task<(bool Success, string? Error)> SendRegistrationOtpAsync(string email, string otp);
    Task<(bool Success, string? Error)> SendPasswordResetOtpAsync(string email, string otp);
}

public class AccountEmailService : IAccountEmailService
{
    private readonly IConfiguration _configuration;

    public AccountEmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<(bool Success, string? Error)> SendRegistrationOtpAsync(string email, string otp)
        => await SendOtpAsync(email, otp, "Xác thực email của bạn", "hoàn tất đăng ký ARS Recruitment");

    public async Task<(bool Success, string? Error)> SendPasswordResetOtpAsync(string email, string otp)
        => await SendOtpAsync(email, otp, "Đặt lại mật khẩu", "đặt lại mật khẩu ARS Recruitment");

    private async Task<(bool Success, string? Error)> SendOtpAsync(string email, string otp, string heading, string action)
    {
        var host = _configuration["EmailOtp:SmtpHost"];
        var username = _configuration["EmailOtp:Username"];
        var password = _configuration["EmailOtp:Password"];
        var fromEmail = _configuration["EmailOtp:FromEmail"] ?? username;
        var fromName = _configuration["EmailOtp:FromName"] ?? "ARS Recruitment";
        var port = _configuration.GetValue<int?>("EmailOtp:Port") ?? 587;

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(fromEmail))
        {
            return (false, "Chưa cấu hình Gmail để gửi OTP. Hãy thêm EmailOtp vào User Secrets.");
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = heading + " | ARS Recruitment";
            message.Body = new BodyBuilder
            {
                HtmlBody = $"<div style='font-family:Arial,sans-serif;color:#172b4d'><h2>{heading}</h2><p>Nhập mã sau để {action}:</p><p style='font-size:30px;font-weight:700;letter-spacing:8px;color:#1267e9'>{otp}</p><p>Mã có hiệu lực trong 10 phút. Không chia sẻ mã này với bất kỳ ai.</p></div>"
            }.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(host, port, port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(username, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, "Không thể gửi OTP qua Gmail: " + ex.Message);
        }
    }
}
