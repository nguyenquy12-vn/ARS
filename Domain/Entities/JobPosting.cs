using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;

namespace Domain.Entities;

public class JobPosting
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CompanyId { get; set; }

    [ForeignKey("CompanyId")]
    public Company? Company { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string Requirements { get; set; } = string.Empty;

    public string? Benefits { get; set; }

    [Required]
    [StringLength(100)]
    public string Location { get; set; } = string.Empty; // Địa điểm cụ thể (Ví dụ: Cầu Giấy, Hà Nội)

    // ==========================================
    // CÁC TRƯỜNG PHỤC VỤ BỘ LỌC (SỬ DỤNG ENUM CHUẨN HÓA)
    // ==========================================

    [Required]
    public JobType JobType { get; set; } = JobType.FullTime; // Full-time, Part-time, Internship...

    [Required]
    public WorkMode WorkMode { get; set; } = WorkMode.Onsite; // Làm trực tiếp, Từ xa, Kết hợp

    [Required]
    public int JobCategoryId { get; set; }

    [ForeignKey("JobCategoryId")]
    public JobCategory? JobCategory { get; set; }

    // Khoảng lương (Null nghĩa là thỏa thuận)
    public int? MinSalary { get; set; }
    public int? MaxSalary { get; set; }

    // Mặc định là Draft theo đề xuất rất hay của bạn
    [Required]
    public JobStatus Status { get; set; } = JobStatus.Draft;

    public int Vacancies { get; set; } = 1;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime ExpiredAt { get; set; }

    public ICollection<Application> Applications { get; set; } = new List<Application>();
}