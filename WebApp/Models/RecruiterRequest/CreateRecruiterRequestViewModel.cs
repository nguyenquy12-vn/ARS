using System.ComponentModel.DataAnnotations;

namespace WebApp.Models.RecruiterRequest;

public class CreateRecruiterRequestViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập tên công ty.")]
    [StringLength(200)]
    [Display(Name = "Tên công ty")]
    public string CompanyName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mã số thuế.")]
    [StringLength(50)]
    [RegularExpression("^[0-9A-Za-z-]{8,50}$", ErrorMessage = "Mã số thuế không hợp lệ.")]
    [Display(Name = "Mã số thuế")]
    public string TaxCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng tải giấy tờ xác minh.")]
    [Display(Name = "Giấy phép kinh doanh/giấy tờ xác minh")]
    public IFormFile? Document { get; set; }
}
