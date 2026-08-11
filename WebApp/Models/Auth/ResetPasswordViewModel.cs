using System.ComponentModel.DataAnnotations;
namespace WebApp.Models.Auth;
public class ResetPasswordViewModel
{
    [Required, RegularExpression("^[0-9]{6}$", ErrorMessage = "OTP gồm 6 chữ số.")]
    public string Otp { get; set; } = string.Empty;
    [Required, StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự.")]
    public string NewPassword { get; set; } = string.Empty;
    [Required, Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
