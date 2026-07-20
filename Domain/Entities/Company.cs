using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class Company
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int RecruiterId { get; set; }

    [ForeignKey("RecruiterId")] 
    public User? Recruiter { get; set; } 

    [Required]
    [StringLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string TaxCode { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Address { get; set; }

    [StringLength(500)]
    public string? LogoPath { get; set; }

    [StringLength(50)]
    public string? CompanySize { get; set; }

    public string? Overview { get; set; } 

    [StringLength(255)]
    [Url] 
    public string? Website { get; set; }
}