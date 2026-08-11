using System.ComponentModel.DataAnnotations;

namespace WebApp.Models.Auth;

public class VerifyEmailOtpViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập mã xác thực.")]
    [RegularExpression("^[0-9]{6}$", ErrorMessage = "Mã OTP gồm đúng 6 chữ số.")]
    public string Otp { get; set; } = string.Empty;
}
