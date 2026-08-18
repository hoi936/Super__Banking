using LocalLink.Application.Common.Models;
using LocalLink.Application.Customers.DTOs;

namespace LocalLink.Application.Customers.Interfaces;

public interface IAdminCustomerService
{
    Task<PagedResult<AdminCustomerListItemDto>> GetCustomersAsync(int page, int pageSize, string? search, string? status, CancellationToken cancellationToken = default);
    Task<AdminCustomerDetailDto> GetCustomerDetailAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<CustomerProfileDto> UpdateCustomerStatusAsync(Guid customerId, UpdateCustomerStatusRequest request, Guid adminUserId, string? ipAddress, CancellationToken cancellationToken = default);
}
