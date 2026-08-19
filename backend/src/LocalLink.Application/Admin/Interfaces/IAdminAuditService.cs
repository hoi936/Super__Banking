using LocalLink.Application.Admin.DTOs;
using LocalLink.Application.Common.Models;

namespace LocalLink.Application.Admin.Interfaces;

public interface IAdminAuditService
{
    Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(
        int page,
        int pageSize,
        string? action = null,
        string? entityType = null,
        Guid? userId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken cancellationToken = default);

    Task<AuditLogDto> GetAuditLogDetailAsync(Guid id, CancellationToken cancellationToken = default);
}
