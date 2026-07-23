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

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<CvBankEntry> CvBankEntries { get; set; } = new List<CvBankEntry>();
}
