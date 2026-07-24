using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [StringLength(20)]
    [Phone] 
    public string? PhoneNumber { get; set; }

    [Required]
    public int RoleId { get; set; }

    [ForeignKey("RoleId")]
    public Role? Role { get; set; }

    [Required]
    public UserStatus Status { get; set; } = UserStatus.Active;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Company? Company { get; set; }

    public ICollection<Application> Applications { get; set; } = new List<Application>();

    // Email verification & external login support
    public bool IsEmailVerified { get; set; } = false;

    [StringLength(100)]
    public string? ExternalProvider { get; set; }

    [StringLength(200)]
    public string? ExternalId { get; set; }
}
