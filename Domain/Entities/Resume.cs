using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class Resume
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CandidateId { get; set; }

    [ForeignKey("CandidateId")]
    public User? Candidate { get; set; }

    [Required]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty; 

    [Required]
    [StringLength(500)]
    public string FilePath { get; set; } = string.Empty; // Đường dẫn vật lý lưu file CV (PDF/Word) trên server hoặc cloud storage

    // Đánh dấu đây có phải là CV chính/mặc định không để hệ thống tự chọn khi ứng viên bấm nộp nhanh
    public bool IsDefault { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // ==========================================
    // CÁC TRƯỜNG PHỤC VỤ CHO BÀI TOÁN AI (GEMINI) CHẤM ĐIỂM
    // ==========================================

    // Lưu toàn bộ nội dung text chữ được trích xuất từ file PDF/Word để làm nguyên liệu đầu vào cho AI đọc
    public string? RawTextContent { get; set; }

    // Danh sách các đơn ứng tuyển sử dụng hồ sơ này (Quan hệ Một - Nhiều với bảng Application)
    public ICollection<Application> Applications { get; set; } = new List<Application>();
}