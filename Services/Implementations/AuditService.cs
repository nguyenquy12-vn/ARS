using Domain.Entities;
using Infrastructure;
using Services.Interfaces;

namespace Services.Implementations;

public class AuditService : IAuditService
{
    private readonly ARSDbContext _context;

    public AuditService(ARSDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(int? actorId, string? actorEmail, string action, string? targetType = null, int? targetId = null, string? description = null)
    {
        var log = new AuditLog
        {
            ActorId = actorId,
            ActorEmail = actorEmail,
            Action = action,
            TargetType = targetType,
            TargetId = targetId,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };

        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();
    }
}
