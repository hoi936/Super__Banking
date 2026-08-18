using LocalLink.Application.Auth.Interfaces;
using LocalLink.Application.Common.Models;
using LocalLink.Application.Notifications.DTOs;
using LocalLink.Application.Notifications.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalLink.API.Controllers;

[ApiController]
[Route("api/v1/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ICurrentUserService _currentUserService;

    public NotificationsController(INotificationService notificationService, ICurrentUserService currentUserService)
    {
        _notificationService = notificationService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Retrieves a paginated list of notifications for the current authenticated user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<NotificationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool? isRead = null,
        [FromQuery] string? type = null,
        CancellationToken cancellationToken = default)
    {
        if (_currentUserService.UserId == null)
        {
            return Unauthorized();
        }

        var result = await _notificationService.GetNotificationsAsync(
            _currentUserService.UserId.Value,
            page,
            pageSize,
            isRead,
            type,
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Retrieves the count of unread notifications for the current authenticated user.
    /// </summary>
    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(UnreadNotificationCountDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            return Unauthorized();
        }

        var result = await _notificationService.GetUnreadCountAsync(_currentUserService.UserId.Value, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves details of a specific notification owned by the current authenticated user.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(NotificationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNotificationDetail(Guid id, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            return Unauthorized();
        }

        var result = await _notificationService.GetNotificationDetailAsync(id, _currentUserService.UserId.Value, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Marks a specific notification as read.
    /// </summary>
    [HttpPatch("{id:guid}/read")]
    [ProducesResponseType(typeof(NotificationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            return Unauthorized();
        }

        var result = await _notificationService.MarkAsReadAsync(id, _currentUserService.UserId.Value, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Marks all unread notifications of the current authenticated user as read.
    /// </summary>
    [HttpPatch("read-all")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            return Unauthorized();
        }

        var count = await _notificationService.MarkAllAsReadAsync(_currentUserService.UserId.Value, cancellationToken);
        return Ok(new { updatedCount = count });
    }
}
