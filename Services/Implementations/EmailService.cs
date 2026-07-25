<<<<<<< HEAD
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Services.Interfaces;
using System.Net;
using System.Net.Mail;

namespace Services.Implementations;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        // Read SMTP config from configuration
        var host = _config["Smtp:Host"];
        var port = int.Parse(_config["Smtp:Port"] ?? "25");
        var user = _config["Smtp:User"];
        var pass = _config["Smtp:Pass"];
        var from = _config["Smtp:From"] ?? user;

        // If no SMTP host configured, fall back to saving emails to disk for local testing
        if (string.IsNullOrWhiteSpace(host))
        {
            try
            {
                var outDir = Path.Combine(Directory.GetCurrentDirectory(), "emails_out");
                Directory.CreateDirectory(outDir);
                var fileName = Path.Combine(outDir, $"email_{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid()}.html");
                var content = $"<h3>To: {to}</h3><h4>Subject: {subject}</h4>" + htmlBody;
                await File.WriteAllTextAsync(fileName, content);
                _logger.LogInformation("Email written to disk at {Path} for {To}", fileName, to);
                return;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to write email to disk");
                throw;
            }
        }

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = bool.Parse(_config["Smtp:EnableSsl"] ?? "true")
        };

        if (!string.IsNullOrEmpty(user))
        {
            client.Credentials = new NetworkCredential(user, pass);
        }

        var mail = new MailMessage(from, to, subject, htmlBody)
        {
            IsBodyHtml = true
        };

        try
        {
            _logger.LogInformation("Sending email to {Email} via SMTP host {Host}:{Port}", to, host, port);
            await client.SendMailAsync(mail);
            _logger.LogInformation("Email sent to {Email}", to);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", to);
            throw;
=======
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
>>>>>>> origin/features
        }
    }
}
