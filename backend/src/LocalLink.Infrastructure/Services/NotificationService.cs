using LocalLink.Application.Common.Exceptions;
using LocalLink.Application.Common.Models;
using LocalLink.Application.Notifications.DTOs;
using LocalLink.Application.Notifications.Interfaces;
using LocalLink.Domain.Enums;
using LocalLink.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LocalLink.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ApplicationDbContext context, ILogger<NotificationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PagedResult<NotificationDto>> GetNotificationsAsync(
        Guid currentUserId,
        int page = 1,
        int pageSize = 10,
        bool? isRead = null,
        string? type = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var query = _context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == currentUserId);

        if (isRead.HasValue)
        {
            query = query.Where(n => n.IsRead == isRead.Value);
        }

        if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<NotificationType>(type, true, out var notifType))
        {
            query = query.Where(n => n.Type == notifType);
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(n => n.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type.ToString().ToUpperInvariant(),
                IsRead = n.IsRead,
                CreatedAtUtc = n.CreatedAtUtc,
                ReadAtUtc = n.ReadAtUtc
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<NotificationDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<NotificationDto> GetNotificationDetailAsync(
        Guid notificationId,
        Guid currentUserId,
        CancellationToken cancellationToken = default)
    {
        var notification = await _context.Notifications
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == currentUserId, cancellationToken);

        if (notification == null)
        {
            throw new NotFoundException("Notification not found.");
        }

        return new NotificationDto
        {
            Id = notification.Id,
            Title = notification.Title,
            Message = notification.Message,
            Type = notification.Type.ToString().ToUpperInvariant(),
            IsRead = notification.IsRead,
            CreatedAtUtc = notification.CreatedAtUtc,
            ReadAtUtc = notification.ReadAtUtc
        };
    }

    public async Task<NotificationDto> MarkAsReadAsync(
        Guid notificationId,
        Guid currentUserId,
        CancellationToken cancellationToken = default)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == currentUserId, cancellationToken);

        if (notification == null)
        {
            throw new NotFoundException("Notification not found.");
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Notification {NotificationId} marked as read for User {UserId}", notificationId, currentUserId);
        }

        return new NotificationDto
        {
            Id = notification.Id,
            Title = notification.Title,
            Message = notification.Message,
            Type = notification.Type.ToString().ToUpperInvariant(),
            IsRead = notification.IsRead,
            CreatedAtUtc = notification.CreatedAtUtc,
            ReadAtUtc = notification.ReadAtUtc
        };
    }

    public async Task<int> MarkAllAsReadAsync(
        Guid currentUserId,
        CancellationToken cancellationToken = default)
    {
        var unreadNotifications = await _context.Notifications
            .Where(n => n.UserId == currentUserId && !n.IsRead)
            .ToListAsync(cancellationToken);

        if (!unreadNotifications.Any())
        {
            return 0;
        }

        var now = DateTime.UtcNow;
        foreach (var n in unreadNotifications)
        {
            n.IsRead = true;
            n.ReadAtUtc = now;
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Marked {Count} notifications as read for User {UserId}", unreadNotifications.Count, currentUserId);

        return unreadNotifications.Count;
    }

    public async Task<UnreadNotificationCountDto> GetUnreadCountAsync(
        Guid currentUserId,
        CancellationToken cancellationToken = default)
    {
        var count = await _context.Notifications
            .AsNoTracking()
            .CountAsync(n => n.UserId == currentUserId && !n.IsRead, cancellationToken);

        return new UnreadNotificationCountDto
        {
            Count = count
        };
    }
}
