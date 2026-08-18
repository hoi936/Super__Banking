using FluentAssertions;
using LocalLink.Application.Auth.DTOs;
using LocalLink.Application.Auth.Interfaces;
using LocalLink.Application.Common.Exceptions;
using LocalLink.Domain.Entities;
using LocalLink.Domain.Enums;
using LocalLink.Infrastructure.Authentication;
using LocalLink.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace LocalLink.UnitTests.Authentication;

public class AuthServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasherService _passwordHasher;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        var jwtOptions = Options.Create(new JwtOptions
        {
            Issuer = "LocalLink",
            Audience = "LocalLink.Client",
            Secret = "SuperSecretDevKeyForUnitTesting_MustBeAtLeast32BytesLong!",
            AccessTokenMinutes = 15,
            RefreshTokenDays = 7
        });

        _tokenService = new TokenService(jwtOptions);
        _passwordHasher = new PasswordHasherService();
        _loggerMock = new Mock<ILogger<AuthService>>();

        _authService = new AuthService(_context, _tokenService, _passwordHasher, _loggerMock.Object);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsLoginResponse_AndCreatesAuditLog()
    {
        // Arrange
        var role = new Role { Id = Guid.NewGuid(), Name = "CUSTOMER" };
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "customer1@locallink.local",
            Status = UserStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, "LocalLink@123");

        var userRole = new UserRole { UserId = user.Id, RoleId = role.Id, Role = role, User = user };
        user.UserRoles.Add(userRole);

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            CustomerCode = "CUS000001",
            FullName = "Nguyen Van An",
            Status = CustomerStatus.Active
        };
        user.Customer = customer;

        _context.Roles.Add(role);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var request = new LoginRequest
        {
            Email = "  CUSTOMER1@LocalLink.Local  ", // Test normalization
            Password = "LocalLink@123"
        };

        // Act
        var response = await _authService.LoginAsync(request, "127.0.0.1");

        // Assert
        response.Should().NotBeNull();
        response.AccessToken.Should().NotBeNullOrWhiteSpace();
        response.RefreshToken.Should().NotBeNullOrWhiteSpace();
        response.User.Email.Should().Be("customer1@locallink.local");
        response.Roles.Should().Contain("CUSTOMER");

        // Verify RefreshToken in DB
        var savedRefreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.UserId == user.Id);
        savedRefreshToken.Should().NotBeNull();
        savedRefreshToken!.RevokedAtUtc.Should().BeNull();

        // Verify AuditLog in DB
        var audit = await _context.AuditLogs.FirstOrDefaultAsync(a => a.UserId == user.Id && a.Action == "LOGIN_SUCCESS");
        audit.Should().NotBeNull();
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ThrowsUnauthorizedException_AndLogsFailure()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "customer1@locallink.local",
            Status = UserStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, "LocalLink@123");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var request = new LoginRequest
        {
            Email = "customer1@locallink.local",
            Password = "WrongPassword"
        };

        // Act & Assert
        var act = async () => await _authService.LoginAsync(request, "127.0.0.1");
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid email or password.");

        var audit = await _context.AuditLogs.FirstOrDefaultAsync(a => a.UserId == user.Id && a.Action == "LOGIN_FAILED");
        audit.Should().NotBeNull();
    }

    [Fact]
    public async Task LoginAsync_WithUnknownEmail_ThrowsUnauthorizedException()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "unknown@locallink.local",
            Password = "LocalLink@123"
        };

        // Act & Assert
        var act = async () => await _authService.LoginAsync(request, "127.0.0.1");
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid email or password.");

        var audit = await _context.AuditLogs.FirstOrDefaultAsync(a => a.Action == "LOGIN_FAILED");
        audit.Should().NotBeNull();
        audit!.UserId.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithSuspendedUser_ThrowsUnauthorizedException()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "suspended@locallink.local",
            Status = UserStatus.Suspended,
            CreatedAtUtc = DateTime.UtcNow
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, "LocalLink@123");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var request = new LoginRequest
        {
            Email = "suspended@locallink.local",
            Password = "LocalLink@123"
        };

        // Act & Assert
        var act = async () => await _authService.LoginAsync(request, "127.0.0.1");
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid email or password.");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_RotatesTokens_AndRevokesOldToken()
    {
        // Arrange
        var role = new Role { Id = Guid.NewGuid(), Name = "CUSTOMER" };
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "customer1@locallink.local",
            Status = UserStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        };
        user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id, Role = role });

        var (rawToken, tokenHash, expiresAtUtc) = _tokenService.GenerateRefreshToken("127.0.0.1");
        var oldTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAtUtc = expiresAtUtc,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.Roles.Add(role);
        _context.Users.Add(user);
        _context.RefreshTokens.Add(oldTokenEntity);
        await _context.SaveChangesAsync();

        var request = new RefreshTokenRequest { RefreshToken = rawToken };

        // Act
        var response = await _authService.RefreshTokenAsync(request, "127.0.0.1");

        // Assert
        response.Should().NotBeNull();
        response.AccessToken.Should().NotBeNullOrWhiteSpace();
        response.RefreshToken.Should().NotBeNullOrWhiteSpace();
        response.RefreshToken.Should().NotBe(rawToken); // Rotated!

        // Old token must be revoked
        var updatedOldToken = await _context.RefreshTokens.FirstAsync(rt => rt.Id == oldTokenEntity.Id);
        updatedOldToken.RevokedAtUtc.Should().NotBeNull();

        // New token must exist and not be revoked
        var newHash = _tokenService.HashToken(response.RefreshToken);
        var newToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == newHash);
        newToken.Should().NotBeNull();
        newToken!.RevokedAtUtc.Should().BeNull();
    }

    [Fact]
    public async Task RefreshTokenAsync_WithRevokedToken_ThrowsUnauthorizedException()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "user@test.local", Status = UserStatus.Active };
        var (rawToken, tokenHash, expiresAtUtc) = _tokenService.GenerateRefreshToken();
        var revokedTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAtUtc = expiresAtUtc,
            RevokedAtUtc = DateTime.UtcNow.AddMinutes(-5),
            CreatedAtUtc = DateTime.UtcNow.AddMinutes(-10)
        };

        _context.Users.Add(user);
        _context.RefreshTokens.Add(revokedTokenEntity);
        await _context.SaveChangesAsync();

        var request = new RefreshTokenRequest { RefreshToken = rawToken };

        // Act & Assert
        var act = async () => await _authService.RefreshTokenAsync(request, "127.0.0.1");
        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task RefreshTokenAsync_WithExpiredToken_ThrowsUnauthorizedException()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "user@test.local", Status = UserStatus.Active };
        var (rawToken, tokenHash, _) = _tokenService.GenerateRefreshToken();
        var expiredTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(-1),
            CreatedAtUtc = DateTime.UtcNow.AddDays(-8)
        };

        _context.Users.Add(user);
        _context.RefreshTokens.Add(expiredTokenEntity);
        await _context.SaveChangesAsync();

        var request = new RefreshTokenRequest { RefreshToken = rawToken };

        // Act & Assert
        var act = async () => await _authService.RefreshTokenAsync(request, "127.0.0.1");
        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task LogoutAsync_RevokesActiveRefreshToken_AndLogsAudit()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "user@test.local", Status = UserStatus.Active };
        var (rawToken, tokenHash, expiresAtUtc) = _tokenService.GenerateRefreshToken();
        var tokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAtUtc = expiresAtUtc,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.Users.Add(user);
        _context.RefreshTokens.Add(tokenEntity);
        await _context.SaveChangesAsync();

        var request = new LogoutRequest { RefreshToken = rawToken };

        // Act
        await _authService.LogoutAsync(request, user.Id, "127.0.0.1");

        // Assert
        var updatedToken = await _context.RefreshTokens.FirstAsync(rt => rt.Id == tokenEntity.Id);
        updatedToken.RevokedAtUtc.Should().NotBeNull();

        var audit = await _context.AuditLogs.FirstOrDefaultAsync(a => a.UserId == user.Id && a.Action == "LOGOUT");
        audit.Should().NotBeNull();
    }

    [Fact]
    public async Task GetCurrentUserAsync_WithActiveUser_ReturnsCurrentUserDto()
    {
        // Arrange
        var role = new Role { Id = Guid.NewGuid(), Name = "ADMIN" };
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@locallink.local",
            Status = UserStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        };
        user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id, Role = role });

        _context.Roles.Add(role);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.GetCurrentUserAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
        result.Email.Should().Be(user.Email);
        result.Roles.Should().Contain("ADMIN");
        result.Customer.Should().BeNull(); // Admin has no customer profile
    }
}
