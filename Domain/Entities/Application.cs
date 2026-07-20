using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums; // Gọi ApplicationStatus đã định nghĩa từ bài trước

namespace Domain.Entities;

public class Application
{
    [Key]
    public int Id { get; set; }

    // 1. Liên kết tới Tin tuyển dụng (One-to-Many)
    [Required]
    public int JobPostingId { get; set; }

    [ForeignKey("JobPostingId")]
    public JobPosting? JobPosting { get; set; }

    // 2. Liên kết tới Ứng viên nộp đơn (One-to-Many)
    [Required]
    public int CandidateId { get; set; }

    [ForeignKey("CandidateId")]
    public User? Candidate { get; set; }

    // 3. Liên kết tới Hồ sơ được sử dụng để nộp (One-to-Many)
    [Required]
    public int ResumeId { get; set; }

    [ForeignKey("ResumeId")]
    public Resume? Resume { get; set; }

    // Thư giới thiệu / Lời nhắn từ ứng viên khi nộp đơn
    [StringLength(1000)]
    public string? CoverLetter { get; set; }

    [Required]
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

    // Trạng thái đơn ứng tuyển (Sử dụng ApplicationStatus Enum: Pending, Reviewing, EvaluatingAI, Accepted, Rejected)
    [Required]
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

    // ==========================================
    // CÁC TRƯỜNG DÀNH CHO GEMINI AI CHẤM ĐIỂM 🌟
    // ==========================================

    // Điểm số AI đánh giá độ phù hợp (Thang điểm 100 hoặc 10 tùy bạn chọn)
    public int? AiMatchScore { get; set; }

    // Đoạn văn ngắn AI nhận xét lý do tại sao đạt/không đạt (Dùng hiển thị cho Recruiter xem)
    public string? AiFeedback { get; set; }
}