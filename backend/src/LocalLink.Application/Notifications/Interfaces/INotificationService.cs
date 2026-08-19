using LocalLink.Application.Common.Models;
using LocalLink.Application.Notifications.DTOs;

namespace LocalLink.Application.Notifications.Interfaces;

public interface INotificationService
{
    Task<PagedResult<NotificationDto>> GetNotificationsAsync(
        Guid currentUserId,
        int page = 1,
        int pageSize = 10,
        bool? isRead = null,
        string? type = null,
        CancellationToken cancellationToken = default);

    Task<NotificationDto> GetNotificationDetailAsync(
        Guid notificationId,
        Guid currentUserId,
        CancellationToken cancellationToken = default);

    Task<NotificationDto> MarkAsReadAsync(
        Guid notificationId,
        Guid currentUserId,
        CancellationToken cancellationToken = default);

    Task<int> MarkAllAsReadAsync(
        Guid currentUserId,
        CancellationToken cancellationToken = default);

    Task<UnreadNotificationCountDto> GetUnreadCountAsync(
        Guid currentUserId,
        CancellationToken cancellationToken = default);
}
