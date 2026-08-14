using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class RecruiterSubscription
{
    [Key]
    public int Id { get; set; }

    public int RecruiterId { get; set; }

    [ForeignKey(nameof(RecruiterId))]
    public User? Recruiter { get; set; }

    [Required, StringLength(30)]
    public string PlanCode { get; set; } = "Starter";

    public DateTime StartedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public int? UpdatedByUserId { get; set; }

    [StringLength(500)]
    public string? AdminNote { get; set; }
}
