using LocalLink.Application.Auth.DTOs;
using LocalLink.Application.Auth.Interfaces;
using LocalLink.Application.Common.Exceptions;
using LocalLink.Domain.Entities;
using LocalLink.Domain.Enums;
using LocalLink.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LocalLink.Infrastructure.Authentication;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasherService _passwordHasher;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        ApplicationDbContext context,
        ITokenService tokenService,
        IPasswordHasherService passwordHasher,
        ILogger<AuthService> logger)
    {
        _context = context;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.Customer)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Login failed: email {Email} not found.", normalizedEmail);

            _context.AuditLogs.Add(new AuditLog
            {
                UserId = null,
                Action = "LOGIN_FAILED",
                EntityType = "User",
                Description = $"Failed login attempt for unknown email: {normalizedEmail}",
                IpAddress = ipAddress,
                CreatedAtUtc = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedException("Invalid email or password.");
        }

        if (user.Status != UserStatus.Active)
        {
            _logger.LogWarning("Login failed: user {UserId} is in state {Status}.", user.Id, user.Status);

            _context.AuditLogs.Add(new AuditLog
            {
                UserId = user.Id,
                Action = "LOGIN_FAILED",
                EntityType = "User",
                EntityId = user.Id.ToString(),
                Description = $"Login attempted on {user.Status} user account",
                IpAddress = ipAddress,
                CreatedAtUtc = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedException("Invalid email or password.");
        }

        var isPasswordValid = _passwordHasher.VerifyPassword(user, user.PasswordHash, request.Password);
        if (!isPasswordValid)
        {
            _logger.LogWarning("Login failed: invalid password for user {UserId}.", user.Id);

            _context.AuditLogs.Add(new AuditLog
            {
                UserId = user.Id,
                Action = "LOGIN_FAILED",
                EntityType = "User",
                EntityId = user.Id.ToString(),
                Description = "Failed login attempt: invalid password",
                IpAddress = ipAddress,
                CreatedAtUtc = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedException("Invalid email or password.");
        }

        user.LastLoginAtUtc = DateTime.UtcNow;

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var (accessToken, expiresAtUtc) = _tokenService.GenerateAccessToken(user, roles, user.Customer?.Id);
        var (rawRefreshToken, tokenHash, refreshExpiresAt) = _tokenService.GenerateRefreshToken(ipAddress);

        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAtUtc = refreshExpiresAt,
            CreatedByIp = ipAddress,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(refreshTokenEntity);

        _context.AuditLogs.Add(new AuditLog
        {
            UserId = user.Id,
            Action = "LOGIN_SUCCESS",
            EntityType = "User",
            EntityId = user.Id.ToString(),
            Description = "User logged in successfully",
            IpAddress = ipAddress,
            CreatedAtUtc = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} logged in successfully.", user.Id);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = rawRefreshToken,
            ExpiresAtUtc = expiresAtUtc,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Status = user.Status.ToString().ToUpperInvariant()
            },
            Roles = roles
        };
    }

    public async Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var tokenHash = _tokenService.HashToken(request.RefreshToken);

        var existingToken = await _context.RefreshTokens
            .Include(rt => rt.User)
                .ThenInclude(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
            .Include(rt => rt.User)
                .ThenInclude(u => u.Customer)
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

        if (existingToken == null)
        {
            _logger.LogWarning("Refresh failed: TokenHash not found.");
            throw new UnauthorizedException("Invalid or expired refresh token.");
        }

        if (existingToken.RevokedAtUtc != null)
        {
            _logger.LogWarning("Refresh failed: Token for user {UserId} has already been revoked.", existingToken.UserId);
            throw new UnauthorizedException("Invalid or expired refresh token.");
        }

        if (existingToken.ExpiresAtUtc <= DateTime.UtcNow)
        {
            _logger.LogWarning("Refresh failed: Token for user {UserId} is expired.", existingToken.UserId);
            throw new UnauthorizedException("Invalid or expired refresh token.");
        }

        if (existingToken.User.Status != UserStatus.Active)
        {
            _logger.LogWarning("Refresh failed: User {UserId} is not Active.", existingToken.UserId);
            throw new UnauthorizedException("User account is not active.");
        }

        // Token Rotation: revoke old token
        existingToken.RevokedAtUtc = DateTime.UtcNow;
        existingToken.RevokedByIp = ipAddress;

        var roles = existingToken.User.UserRoles.Select(ur => ur.Role.Name).ToList();
        var (newAccessToken, expiresAtUtc) = _tokenService.GenerateAccessToken(existingToken.User, roles, existingToken.User.Customer?.Id);
        var (newRawRefreshToken, newTokenHash, newRefreshExpiresAt) = _tokenService.GenerateRefreshToken(ipAddress);

        var newRefreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = existingToken.UserId,
            TokenHash = newTokenHash,
            ExpiresAtUtc = newRefreshExpiresAt,
            CreatedByIp = ipAddress,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(newRefreshTokenEntity);

        _context.AuditLogs.Add(new AuditLog
        {
            UserId = existingToken.UserId,
            Action = "TOKEN_REFRESH",
            EntityType = "User",
            EntityId = existingToken.UserId.ToString(),
            Description = "Access and refresh tokens rotated successfully",
            IpAddress = ipAddress,
            CreatedAtUtc = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Token refreshed successfully for user {UserId}.", existingToken.UserId);

        return new TokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRawRefreshToken,
            ExpiresAtUtc = expiresAtUtc
        };
    }

    public async Task LogoutAsync(LogoutRequest request, Guid userId, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            var tokenHash = _tokenService.HashToken(request.RefreshToken);
            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash && rt.UserId == userId, cancellationToken);

            if (token != null && token.RevokedAtUtc == null)
            {
                token.RevokedAtUtc = DateTime.UtcNow;
                token.RevokedByIp = ipAddress;
            }
        }

        _context.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = "LOGOUT",
            EntityType = "User",
            EntityId = userId.ToString(),
            Description = "User logged out and refresh token revoked",
            IpAddress = ipAddress,
            CreatedAtUtc = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} logged out successfully.", userId);
    }

    public async Task<CurrentUserDto> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.Customer)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null || user.Status != UserStatus.Active)
        {
            throw new UnauthorizedException("User not found or inactive.");
        }

        return new CurrentUserDto
        {
            Id = user.Id,
            Email = user.Email,
            Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList(),
            Customer = user.Customer == null ? null : new CustomerProfileDto
            {
                Id = user.Customer.Id,
                CustomerCode = user.Customer.CustomerCode,
                FullName = user.Customer.FullName
            }
        };
    }
}
