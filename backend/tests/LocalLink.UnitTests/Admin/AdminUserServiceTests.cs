using LocalLink.Application.Admin.DTOs;
using LocalLink.Application.Common.Exceptions;
using LocalLink.Domain.Entities;
using LocalLink.Domain.Enums;
using LocalLink.Infrastructure.Persistence;
using LocalLink.Infrastructure.Services.Admin;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LocalLink.UnitTests.Admin;

public class AdminUserServiceTests
{
    private readonly DbContextOptions<ApplicationDbContext> _options;

    public AdminUserServiceTests()
    {
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task UpdateUserStatus_ShouldUpdateStatusAndLogAudit_WhenValid()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var loggerMock = new Mock<ILogger<AdminUserService>>();
        var service = new AdminUserService(context, loggerMock.Object);

        var adminId = Guid.NewGuid();
        var targetUser = new User { Id = Guid.NewGuid(), Email = "test@locallink.local", Status = UserStatus.Active };
        context.Users.Add(targetUser);
        await context.SaveChangesAsync();

        var request = new UpdateUserStatusRequest { Status = UserStatus.Suspended, Reason = "Violation" };

        // Act
        var result = await service.UpdateUserStatusAsync(targetUser.Id, request, adminId, "127.0.0.1");

        // Assert
        Assert.Equal(UserStatus.Suspended, result.Status);
        
        var userInDb = await context.Users.FindAsync(targetUser.Id);
        Assert.Equal(UserStatus.Suspended, userInDb!.Status);

        var auditLog = await context.AuditLogs.FirstOrDefaultAsync(a => a.EntityId == targetUser.Id.ToString());
        Assert.NotNull(auditLog);
        Assert.Equal("USER_SUSPEND", auditLog.Action);
        Assert.Equal(adminId, auditLog.UserId);
    }

    [Fact]
    public async Task UpdateUserStatus_ShouldRevokeRefreshTokens_WhenSuspended()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var loggerMock = new Mock<ILogger<AdminUserService>>();
        var service = new AdminUserService(context, loggerMock.Object);

        var targetUser = new User { Id = Guid.NewGuid(), Email = "test@locallink.local", Status = UserStatus.Active };
        context.Users.Add(targetUser);

        var token = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = targetUser.Id,
            TokenHash = "hash",
            ExpiresAtUtc = DateTime.UtcNow.AddDays(1)
        };
        context.RefreshTokens.Add(token);
        await context.SaveChangesAsync();

        var request = new UpdateUserStatusRequest { Status = UserStatus.Suspended };

        // Act
        await service.UpdateUserStatusAsync(targetUser.Id, request, Guid.NewGuid(), null);

        // Assert
        var updatedToken = await context.RefreshTokens.FindAsync(token.Id);
        Assert.NotNull(updatedToken!.RevokedAtUtc);
    }

    [Fact]
    public async Task UpdateUserStatus_ShouldThrowValidationException_WhenSelfSuspending()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var loggerMock = new Mock<ILogger<AdminUserService>>();
        var service = new AdminUserService(context, loggerMock.Object);

        var adminId = Guid.NewGuid();
        var adminUser = new User { Id = adminId, Email = "admin@locallink.local", Status = UserStatus.Active };
        context.Users.Add(adminUser);
        await context.SaveChangesAsync();

        var request = new UpdateUserStatusRequest { Status = UserStatus.Suspended };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => 
            service.UpdateUserStatusAsync(adminId, request, adminId, null));
        
        Assert.Contains("cannot suspend or alter their own status", exception.Message);
    }
}
