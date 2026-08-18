using FluentAssertions;
using LocalLink.Application.Common.Exceptions;
using LocalLink.Domain.Entities;
using LocalLink.Domain.Enums;
using LocalLink.Infrastructure.Persistence;
using LocalLink.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LocalLink.UnitTests.Notifications;

public class NotificationServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly NotificationService _service;
    private readonly Mock<ILogger<NotificationService>> _loggerMock;

    public NotificationServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<NotificationService>>();
        _service = new NotificationService(_context, _loggerMock.Object);
    }

    [Fact]
    public async Task GetNotificationsAsync_ReturnsOnlyCurrentUserNotifications_SortedNewestFirst()
    {
        // Arrange
        var user1 = new User { Id = Guid.NewGuid(), Email = "user1@locallink.local", Status = UserStatus.Active };
        var user2 = new User { Id = Guid.NewGuid(), Email = "user2@locallink.local", Status = UserStatus.Active };

        var notif1 = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = user1.Id,
            Title = "Old",
            Message = "Old message",
            Type = NotificationType.System,
            CreatedAtUtc = DateTime.UtcNow.AddHours(-2)
        };
        var notif2 = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = user1.Id,
            Title = "New",
            Message = "New message",
            Type = NotificationType.Transfer,
            CreatedAtUtc = DateTime.UtcNow
        };
        var notif3 = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = user2.Id,
            Title = "User2 notif",
            Message = "For user 2",
            Type = NotificationType.Payment,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.Users.AddRange(user1, user2);
        _context.Notifications.AddRange(notif1, notif2, notif3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetNotificationsAsync(user1.Id, page: 1, pageSize: 10);

        // Assert
        result.Should().NotBeNull();
        result.TotalItems.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.Items[0].Title.Should().Be("New"); // Newest first
        result.Items[1].Title.Should().Be("Old");
        result.Items.Select(x => x.Title).Should().NotContain("User2 notif");
    }

    [Fact]
    public async Task MarkAsReadAsync_UpdatesIsReadAndReadAtUtc_Idempotent()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "user@locallink.local", Status = UserStatus.Active };
        var notif = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Title = "Test Notif",
            Message = "Unread",
            Type = NotificationType.Payment,
            IsRead = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.Users.Add(user);
        _context.Notifications.Add(notif);
        await _context.SaveChangesAsync();

        // Act - First call
        var updated = await _service.MarkAsReadAsync(notif.Id, user.Id);

        // Assert
        updated.IsRead.Should().BeTrue();
        updated.ReadAtUtc.Should().NotBeNull();

        // Act - Second call (idempotent)
        var secondCall = await _service.MarkAsReadAsync(notif.Id, user.Id);
        secondCall.IsRead.Should().BeTrue();
    }

    [Fact]
    public async Task MarkAllAsReadAsync_UpdatesAllUnreadForCurrentUser()
    {
        // Arrange
        var user1 = new User { Id = Guid.NewGuid(), Email = "user1@locallink.local", Status = UserStatus.Active };
        var user2 = new User { Id = Guid.NewGuid(), Email = "user2@locallink.local", Status = UserStatus.Active };

        var notif1 = new Notification { Id = Guid.NewGuid(), UserId = user1.Id, Title = "N1", Message = "M1", IsRead = false };
        var notif2 = new Notification { Id = Guid.NewGuid(), UserId = user1.Id, Title = "N2", Message = "M2", IsRead = false };
        var notifUser2 = new Notification { Id = Guid.NewGuid(), UserId = user2.Id, Title = "N3", Message = "M3", IsRead = false };

        _context.Users.AddRange(user1, user2);
        _context.Notifications.AddRange(notif1, notif2, notifUser2);
        await _context.SaveChangesAsync();

        // Act
        var count = await _service.MarkAllAsReadAsync(user1.Id);

        // Assert
        count.Should().Be(2);

        var unreadUser1 = await _context.Notifications.CountAsync(n => n.UserId == user1.Id && !n.IsRead);
        unreadUser1.Should().Be(0);

        // User2 unread should remain untouched
        var unreadUser2 = await _context.Notifications.CountAsync(n => n.UserId == user2.Id && !n.IsRead);
        unreadUser2.Should().Be(1);
    }

    [Fact]
    public async Task GetUnreadCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "user@locallink.local", Status = UserStatus.Active };
        var notif1 = new Notification { Id = Guid.NewGuid(), UserId = user.Id, Title = "N1", Message = "M1", IsRead = false };
        var notif2 = new Notification { Id = Guid.NewGuid(), UserId = user.Id, Title = "N2", Message = "M2", IsRead = true };
        var notif3 = new Notification { Id = Guid.NewGuid(), UserId = user.Id, Title = "N3", Message = "M3", IsRead = false };

        _context.Users.Add(user);
        _context.Notifications.AddRange(notif1, notif2, notif3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetUnreadCountAsync(user.Id);

        // Assert
        result.Count.Should().Be(2);
    }

    [Fact]
    public async Task Notification_OwnershipEnforced_UserCannotReadOrMarkOthersNotification()
    {
        // Arrange
        var user1 = new User { Id = Guid.NewGuid(), Email = "user1@locallink.local", Status = UserStatus.Active };
        var user2 = new User { Id = Guid.NewGuid(), Email = "user2@locallink.local", Status = UserStatus.Active };
        var notif2 = new Notification { Id = Guid.NewGuid(), UserId = user2.Id, Title = "Secret", Message = "User2 only", IsRead = false };

        _context.Users.AddRange(user1, user2);
        _context.Notifications.Add(notif2);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetNotificationDetailAsync(notif2.Id, user1.Id));
        await Assert.ThrowsAsync<NotFoundException>(() => _service.MarkAsReadAsync(notif2.Id, user1.Id));
    }
}
