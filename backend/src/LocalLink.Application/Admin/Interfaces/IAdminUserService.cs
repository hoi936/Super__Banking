using LocalLink.Application.Admin.DTOs;
using LocalLink.Application.Common.Models;
using LocalLink.Domain.Enums;

namespace LocalLink.Application.Admin.Interfaces;

public interface IAdminUserService
{
    Task<PagedResult<AdminUserListItemDto>> GetUsersAsync(
        int page,
        int pageSize,
        string? search = null,
        UserStatus? status = null,
        string? role = null,
        CancellationToken cancellationToken = default);

    Task<AdminUserDetailDto> GetUserDetailAsync(Guid id, CancellationToken cancellationToken = default);

    Task<AdminUserDetailDto> UpdateUserStatusAsync(
        Guid id,
        UpdateUserStatusRequest request,
        Guid currentAdminId,
        string? ipAddress,
        CancellationToken cancellationToken = default);
}
