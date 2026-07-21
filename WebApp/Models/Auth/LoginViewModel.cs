using System.ComponentModel.DataAnnotations;

namespace WebApp.Models.Auth;

public class LoginViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập Email")]
    [EmailAddress(ErrorMessage = "Định dạng Email không hợp lệ")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập Mật khẩu")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    // Tính năng mở rộng rất hay dùng ở màn hình Login:
    public bool RememberMe { get; set; }

}
