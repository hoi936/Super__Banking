using LocalLink.Application.Admin.DTOs;
using LocalLink.Application.Admin.Interfaces;
using LocalLink.Application.Common.Exceptions;
using LocalLink.Application.Common.Models;
using LocalLink.Domain.Entities;
using LocalLink.Domain.Enums;
using LocalLink.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LocalLink.Infrastructure.Services.Admin;

public class AdminUserService : IAdminUserService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AdminUserService> _logger;

    public AdminUserService(ApplicationDbContext context, ILogger<AdminUserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PagedResult<AdminUserListItemDto>> GetUsersAsync(
        int page,
        int pageSize,
        string? search = null,
        UserStatus? status = null,
        string? role = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var query = _context.Users
            .Include(u => u.Customer)
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(u => 
                u.Email.Contains(search) || 
                (u.Customer != null && (u.Customer.CustomerCode.Contains(search) || u.Customer.FullName.Contains(search))));
        }

        if (status.HasValue) query = query.Where(u => u.Status == status.Value);
        
        if (!string.IsNullOrWhiteSpace(role))
        {
            query = query.Where(u => u.UserRoles.Any(ur => ur.Role!.Name == role.ToUpper()));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(u => u.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new AdminUserListItemDto
            {
                Id = u.Id,
                Email = u.Email,
                Status = u.Status,
                Roles = u.UserRoles.Select(ur => ur.Role!.Name).ToList(),
                CustomerCode = u.Customer != null ? u.Customer.CustomerCode : null,
                FullName = u.Customer != null ? u.Customer.FullName : null,
                CreatedAtUtc = u.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<AdminUserListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<AdminUserDetailDto> GetUserDetailAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.Customer)
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user == null)
            throw new NotFoundException("User not found");

        return new AdminUserDetailDto
        {
            Id = user.Id,
            Email = user.Email,
            Status = user.Status,
            Roles = user.UserRoles.Select(ur => ur.Role!.Name).ToList(),
            CustomerCode = user.Customer?.CustomerCode,
            FullName = user.Customer?.FullName,
            CreatedAtUtc = user.CreatedAtUtc,
            // LastLoginAtUtc could be added to entity in future if needed
        };
    }

    public async Task<AdminUserDetailDto> UpdateUserStatusAsync(
        Guid id,
        UpdateUserStatusRequest request,
        Guid currentAdminId,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        if (id == currentAdminId)
        {
            throw new BadRequestException("An administrator cannot suspend or alter their own status.");
        }

        var user = await _context.Users
            .Include(u => u.Customer)
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user == null)
            throw new NotFoundException("User not found");

        if (user.Status == request.Status)
            throw new BadRequestException($"User is already in status: {request.Status}");

        var oldStatus = user.Status;
        user.Status = request.Status;

        string action = request.Status == UserStatus.Suspended ? "USER_SUSPEND" :
                        request.Status == UserStatus.Active ? "USER_ACTIVATE" : $"USER_STATUS_{request.Status.ToString().ToUpper()}";

        // Audit Log
        var auditLog = new AuditLog
        {
            UserId = currentAdminId,
            Action = action,
            EntityType = "User",
            EntityId = user.Id.ToString(),
            Description = $"Status changed from {oldStatus} to {user.Status}. Reason: {request.Reason}",
            IpAddress = ipAddress,
            CreatedAtUtc = DateTime.UtcNow
        };
        _context.AuditLogs.Add(auditLog);

        // If suspended, revoke active refresh tokens
        if (user.Status == UserStatus.Suspended)
        {
            var activeTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == user.Id && rt.RevokedAtUtc == null && rt.ExpiresAtUtc > DateTime.UtcNow)
                .ToListAsync(cancellationToken);

            foreach (var token in activeTokens)
            {
                token.RevokedAtUtc = DateTime.UtcNow;
            }
            _logger.LogInformation("Revoked {Count} refresh tokens for suspended user {UserId}", activeTokens.Count, user.Id);
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Admin {AdminId} updated user {UserId} status to {Status}", currentAdminId, user.Id, user.Status);

        return new AdminUserDetailDto
        {
            Id = user.Id,
            Email = user.Email,
            Status = user.Status,
            Roles = user.UserRoles.Select(ur => ur.Role!.Name).ToList(),
            CustomerCode = user.Customer?.CustomerCode,
            FullName = user.Customer?.FullName,
            CreatedAtUtc = user.CreatedAtUtc
        };
    }
}
