 using Domain.Entities;

namespace Services.Interfaces;

public interface INotificationService
{
    Task CreateAsync(int userId, string title, string message, string type, int? relatedId = null);
    Task<List<Notification>> GetByUserAsync(int userId);
    Task<int> GetUnreadCountAsync(int userId);
    Task MarkAsReadAsync(int notificationId, int userId);
    Task MarkAllAsReadAsync(int userId);
}
