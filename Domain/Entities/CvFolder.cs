using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

// Thư mục phân loại CV trong Kho CV của một Nhà tuyển dụng (vd: "Backend", "Fresher", "Đã phỏng vấn").
public class CvFolder
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int RecruiterId { get; set; }

    [ForeignKey("RecruiterId")]
    public User? Recruiter { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    // ===== JD của thư mục (dùng để chấm điểm các CV bên trong) =====
    public string? JdDescription { get; set; }   // Mô tả vị trí
    public string? JdRequirements { get; set; }  // Yêu cầu

    // Cài đặt trọng số chấm điểm (giống tin tuyển dụng)
    public int AiWeightExperience { get; set; } = 35;
    public int AiWeightSkills { get; set; } = 40;
    public int AiWeightEducation { get; set; } = 10;
    public int AiWeightAchievement { get; set; } = 15;
    public string? AiPriorityNote { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<CvBankEntry> CvBankEntries { get; set; } = new List<CvBankEntry>();
}
