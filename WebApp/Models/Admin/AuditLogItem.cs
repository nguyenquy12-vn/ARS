namespace WebApp.Models.Admin;

public class AuditLogItem
{
    public int Id { get; set; }
    public int? ActorId { get; set; }
    public string? ActorEmail { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? TargetType { get; set; }
    public int? TargetId { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
