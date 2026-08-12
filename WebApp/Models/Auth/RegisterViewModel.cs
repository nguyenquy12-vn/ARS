using System.ComponentModel.DataAnnotations;

namespace WebApp.Models.Auth;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Định dạng Email không hợp lệ")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự")]
    [RegularExpression("^(?=.*[A-Z])(?=.*\\d).{8,}$", ErrorMessage = "Mật khẩu phải có ít nhất 1 chữ in hoa và 1 chữ số.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Xác nhận mật khẩu không được để trống")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Họ và tên không được để trống")]
    public string FullName { get; set; } = string.Empty;

}
