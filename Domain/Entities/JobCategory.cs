using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class JobCategory
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty; // Tên ngành nghề (Ví dụ: Công nghệ thông tin)

    [StringLength(250)]
    public string? Description { get; set; } // Mô tả ngắn về ngành nghề nếu cần

    // Thuộc tính điều hướng đảo: Một ngành nghề có nhiều tin tuyển dụng
    public ICollection<JobPosting> JobPostings { get; set; } = new List<JobPosting>();
}