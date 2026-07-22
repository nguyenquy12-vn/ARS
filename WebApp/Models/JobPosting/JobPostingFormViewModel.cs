using System.ComponentModel.DataAnnotations;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.Models.JobPosting;

public class JobPostingFormViewModel : IValidatableObject
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tiêu đề tin tuyển dụng")]
    [StringLength(200, ErrorMessage = "Tiêu đề tối đa 200 ký tự")]
    [Display(Name = "Tiêu đề")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mô tả công việc")]
    [Display(Name = "Mô tả công việc")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập yêu cầu ứng viên")]
    [Display(Name = "Yêu cầu ứng viên")]
    public string Requirements { get; set; } = string.Empty;

    [Display(Name = "Quyền lợi")]
    public string? Benefits { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập địa điểm làm việc")]
    [StringLength(100, ErrorMessage = "Địa điểm tối đa 100 ký tự")]
    [Display(Name = "Địa điểm")]
    public string Location { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Loại hình")]
    public JobType JobType { get; set; } = JobType.FullTime;

    [Required]
    [Display(Name = "Hình thức làm việc")]
    public WorkMode WorkMode { get; set; } = WorkMode.Onsite;

    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn ngành nghề")]
    [Display(Name = "Ngành nghề")]
    public int JobCategoryId { get; set; }

    [Required]
    [Display(Name = "Trạng thái")]
    public JobStatus Status { get; set; } = JobStatus.Draft;

    [Range(0, int.MaxValue, ErrorMessage = "Lương tối thiểu không hợp lệ")]
    [Display(Name = "Lương tối thiểu (VNĐ)")]
    public int? MinSalary { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Lương tối đa không hợp lệ")]
    [Display(Name = "Lương tối đa (VNĐ)")]
    public int? MaxSalary { get; set; }

    [Range(1, 1000, ErrorMessage = "Số lượng tuyển phải từ 1 trở lên")]
    [Display(Name = "Số lượng tuyển")]
    public int Vacancies { get; set; } = 1;

    [Required(ErrorMessage = "Vui lòng chọn hạn nộp hồ sơ")]
    [DataType(DataType.Date)]
    [Display(Name = "Hạn nộp hồ sơ")]
    public DateTime ExpiredAt { get; set; } = DateTime.Today.AddDays(30);

    // Dữ liệu cho dropdown ngành nghề (không tham gia validate)
    public IEnumerable<SelectListItem> Categories { get; set; } = new List<SelectListItem>();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (MinSalary.HasValue && MaxSalary.HasValue && MaxSalary.Value < MinSalary.Value)
        {
            yield return new ValidationResult(
                "Lương tối đa phải lớn hơn hoặc bằng lương tối thiểu",
                new[] { nameof(MaxSalary) });
        }

        if (ExpiredAt.Date < DateTime.Today)
        {
            yield return new ValidationResult(
                "Hạn nộp hồ sơ phải là ngày trong tương lai",
                new[] { nameof(ExpiredAt) });
        }
    }
}
