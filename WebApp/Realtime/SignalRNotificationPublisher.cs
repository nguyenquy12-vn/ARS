using Microsoft.AspNetCore.SignalR;
using Services.Interfaces;

namespace WebApp.Realtime;

public sealed class SignalRNotificationPublisher : IRealtimeNotificationPublisher
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public SignalRNotificationPublisher(IHubContext<NotificationHub> hubContext) => _hubContext = hubContext;

    public Task PublishAsync(int userId, string title, string message, string type, int? relatedId)
    {
        return _hubContext.Clients.Group($"user:{userId}").SendAsync("notification:new", new
        {
            title,
            message,
            type,
            relatedId
        });
    }
}
