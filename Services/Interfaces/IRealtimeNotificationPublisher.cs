namespace Services.Interfaces;

public interface IRealtimeNotificationPublisher
{
    Task PublishAsync(int userId, string title, string message, string type, int? relatedId);
}
