using LocalLink.Application.Admin.DTOs;

namespace LocalLink.Application.Admin.Interfaces;

public interface IAdminDashboardService
{
    Task<AdminDashboardDto> GetDashboardMetricsAsync(CancellationToken cancellationToken = default);
}
