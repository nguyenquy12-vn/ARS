using System.ComponentModel.DataAnnotations;

namespace WebApp.Models.Company;

public class CompanyFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên công ty")]
    [StringLength(200, ErrorMessage = "Tên công ty tối đa 200 ký tự")]
    [Display(Name = "Tên công ty")]
    public string CompanyName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mã số thuế")]
    [StringLength(50, ErrorMessage = "Mã số thuế tối đa 50 ký tự")]
    [Display(Name = "Mã số thuế")]
    public string TaxCode { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "Địa chỉ tối đa 255 ký tự")]
    [Display(Name = "Địa chỉ")]
    public string? Address { get; set; }

    [StringLength(500, ErrorMessage = "Đường dẫn logo tối đa 500 ký tự")]
    [Display(Name = "Logo (đường dẫn)")]
    public string? LogoPath { get; set; }

    [StringLength(50, ErrorMessage = "Quy mô tối đa 50 ký tự")]
    [Display(Name = "Quy mô công ty")]
    public string? CompanySize { get; set; }

    [Display(Name = "Giới thiệu công ty")]
    public string? Overview { get; set; }

    [StringLength(255, ErrorMessage = "Website tối đa 255 ký tự")]
    [Url(ErrorMessage = "Website không hợp lệ")]
    [Display(Name = "Website")]
    public string? Website { get; set; }
}
