using LocalLink.Application.Admin.DTOs;
using LocalLink.Application.Admin.Interfaces;
using LocalLink.Application.Common.Exceptions;
using LocalLink.Application.Common.Models;
using LocalLink.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LocalLink.Infrastructure.Services.Admin;

public class AdminAuditService : IAdminAuditService
{
    private readonly ApplicationDbContext _context;

    public AdminAuditService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(
        int page,
        int pageSize,
        string? action = null,
        string? entityType = null,
        Guid? userId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;
        if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
            throw new BadRequestException("FromDate cannot be greater than ToDate");

        var query = _context.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(a => 
                (a.Description != null && a.Description.Contains(search)) || 
                (a.IpAddress != null && a.IpAddress.Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(action)) query = query.Where(a => a.Action.Contains(action));
        if (!string.IsNullOrWhiteSpace(entityType)) query = query.Where(a => a.EntityType == entityType);
        if (userId.HasValue) query = query.Where(a => a.UserId == userId.Value);
        
        if (fromDate.HasValue) query = query.Where(a => a.CreatedAtUtc >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(a => a.CreatedAtUtc <= toDate.Value);

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(a => a.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AuditLogDto
            {
                Id = a.Id,
                UserId = a.UserId,
                Action = a.Action,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                Description = a.Description,
                IpAddress = a.IpAddress,
                CreatedAtUtc = a.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<AuditLogDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<AuditLogDto> GetAuditLogDetailAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var log = await _context.AuditLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (log == null)
            throw new NotFoundException("Audit log not found");

        return new AuditLogDto
        {
            Id = log.Id,
            UserId = log.UserId,
            Action = log.Action,
            EntityType = log.EntityType,
            EntityId = log.EntityId,
            Description = log.Description,
            IpAddress = log.IpAddress,
            CreatedAtUtc = log.CreatedAtUtc
        };
    }
}
