using System.ComponentModel.DataAnnotations;

namespace WebApp.Models.Profile;

public class ProfileViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ tên")]
    [StringLength(100)]
    [Display(Name = "Họ tên")]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "Email đăng nhập")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [StringLength(20)]
    [Display(Name = "Số điện thoại")]
    public string? PhoneNumber { get; set; }

    // ===== Cài đặt email (SMTP) =====
    [Display(Name = "SMTP Host")]
    [StringLength(150)]
    public string? SmtpHost { get; set; }

    [Display(Name = "SMTP Port")]
    public int? SmtpPort { get; set; }

    [Display(Name = "SMTP Username (email)")]
    [StringLength(150)]
    public string? SmtpUsername { get; set; }

    [Display(Name = "SMTP Password (App Password)")]
    [StringLength(255)]
    public string? SmtpPassword { get; set; }

    [Display(Name = "Email người gửi")]
    [StringLength(150)]
    public string? SmtpFromEmail { get; set; }

    [Display(Name = "Dùng SSL/TLS")]
    public bool SmtpEnableSsl { get; set; } = true;
}
