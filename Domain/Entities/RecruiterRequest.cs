using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class RecruiterRequest
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }
    [ForeignKey("UserId")]
    public User? User { get; set; } 

    [Required]
    [StringLength(200)]
    public string CompanyName { get; set; } = string.Empty; 

    [Required]
    [StringLength(50)]
    public string TaxCode { get; set; } = string.Empty;

    [Required]
    public string DocumentPath { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public RecruiterRequestStatus Status { get; set; } = RecruiterRequestStatus.Pending;

    public string? AdminNotes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
}