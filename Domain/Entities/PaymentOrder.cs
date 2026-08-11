using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;

namespace Domain.Entities;

public class PaymentOrder
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int RecruiterId { get; set; }

    [ForeignKey(nameof(RecruiterId))]
    public User? Recruiter { get; set; }

    [Required, StringLength(30)]
    public string PlanCode { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string PlanName { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    [Required, StringLength(40)]
    public string TransferCode { get; set; } = string.Empty;

    public PaymentStatus Status { get; set; } = PaymentStatus.PendingConfirmation;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
    public int? ReviewedByUserId { get; set; }

    [StringLength(500)]
    public string? AdminNote { get; set; }
}
