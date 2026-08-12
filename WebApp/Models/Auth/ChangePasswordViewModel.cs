using System.ComponentModel.DataAnnotations;
namespace WebApp.Models.Auth;
public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại.")] public string CurrentPassword { get; set; } = string.Empty;
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới."), StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu mới phải có ít nhất 8 ký tự."), RegularExpression("^(?=.*[A-Z])(?=.*\\d).{8,}$", ErrorMessage = "Mật khẩu phải có ít nhất 1 chữ in hoa và 1 chữ số.")] public string NewPassword { get; set; } = string.Empty;
    [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới."), Compare(nameof(NewPassword), ErrorMessage = "Xác nhận mật khẩu không khớp.")] public string ConfirmPassword { get; set; } = string.Empty;
}
