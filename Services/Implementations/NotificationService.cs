using Domain.Entities;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Services.Interfaces;

namespace Services.Implementations;

public class NotificationService : INotificationService
{
    private readonly ARSDbContext _context;

    public NotificationService(ARSDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(int userId, string title, string message, string type, int? relatedId = null)
    {
        _context.Notifications.Add(new Notification
        {
            UserId    = userId,
            Title     = title,
            Message   = message,
            Type      = type,
            RelatedId = relatedId,
            IsRead    = false,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
    }

    public async Task<List<Notification>> GetByUserAsync(int userId)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task MarkAsReadAsync(int notificationId, int userId)
    {
        var n = await _context.Notifications
            .FirstOrDefaultAsync(x => x.Id == notificationId && x.UserId == userId);
        if (n is null) return;
        n.IsRead = true;
        await _context.SaveChangesAsync();
    }

    public async Task MarkAllAsReadAsync(int userId)
    {
        await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
    }
}
