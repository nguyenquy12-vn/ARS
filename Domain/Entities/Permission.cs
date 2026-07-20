using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Permission
{
    [Key]
    public int Id { get; set; } 

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

}