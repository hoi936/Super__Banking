using LocalLink.Application.Customers.DTOs;

namespace LocalLink.Application.Customers.Interfaces;

public interface ICustomerService
{
    Task<CustomerProfileDto> GetMyProfileAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<CustomerProfileDto> UpdateMyProfileAsync(Guid userId, UpdateCustomerProfileRequest request, CancellationToken cancellationToken = default);
}
