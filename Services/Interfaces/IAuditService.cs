namespace Services.Interfaces;

public interface IAuditService
{
    Task LogAsync(int? actorId, string? actorEmail, string action, string? targetType = null, int? targetId = null, string? description = null);
}
