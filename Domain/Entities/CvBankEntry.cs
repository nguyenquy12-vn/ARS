using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

// Một CV trong "Kho CV" của Nhà tuyển dụng: file PDF được upload, đọc text rồi cho AI (Gemini)
// trích xuất thông tin có cấu trúc để lọc/tìm kiếm nhanh.
public class CvBankEntry
{
    [Key]
    public int Id { get; set; }

    // Nhà tuyển dụng sở hữu CV này
    [Required]
    public int RecruiterId { get; set; }

    [ForeignKey("RecruiterId")]
    public User? Recruiter { get; set; }

    // Thư mục phân loại (tùy chọn). Null = chưa phân loại ("Tất cả").
    public int? FolderId { get; set; }

    [ForeignKey("FolderId")]
    public CvFolder? Folder { get; set; }

    // Tên file gốc do người dùng upload (để hiển thị)
    [Required]
    [StringLength(255)]
    public string FileName { get; set; } = string.Empty;

    // Tên file lưu trên đĩa (GUID.pdf) dưới wwwroot/uploads/cvbank
    [Required]
    [StringLength(255)]
    public string StoredFileName { get; set; } = string.Empty;

    // ===== Thông tin do AI trích xuất =====
    [StringLength(150)]
    public string? Name { get; set; }

    [StringLength(150)]
    public string? Email { get; set; }

    [StringLength(50)]
    public string? Phone { get; set; }

    [StringLength(200)]
    public string? CurrentTitle { get; set; }

    // Số năm kinh nghiệm làm việc tổng
    public double TotalYearsExperience { get; set; }

    // Số năm kinh nghiệm liên quan AI/ML/Data
    public double AiYearsExperience { get; set; }

    public bool IsFresher { get; set; }

    // Danh sách kỹ năng, lưu dạng chuỗi ngăn cách bởi dấu phẩy
    public string? Skills { get; set; }

    // Tóm tắt ngắn về ứng viên
    public string? Summary { get; set; }

    // Điểm mạnh / điểm yếu (mỗi mục 1 dòng)
    public string? Strengths { get; set; }
    public string? Weaknesses { get; set; }

    // Toàn bộ text đọc từ PDF (nguyên liệu cho AI)
    public string? RawText { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
