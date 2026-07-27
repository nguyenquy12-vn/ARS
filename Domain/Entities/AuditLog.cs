using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class AuditLog
{
    [Key]
    public int Id { get; set; }

    // Actor performing the action (nullable if system)
    public int? ActorId { get; set; }

    [StringLength(150)]
    public string? ActorEmail { get; set; }

    [Required]
    [StringLength(100)]
    public string Action { get; set; } = string.Empty;

    [StringLength(100)]
    public string? TargetType { get; set; }

    public int? TargetId { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
