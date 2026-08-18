using LocalLink.Application.Accounts.DTOs;

namespace LocalLink.Application.Accounts.Interfaces;

public interface IAdminAccountService
{
    Task<IReadOnlyList<AccountSummaryDto>> GetCustomerAccountsAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<AccountSummaryDto> UpdateAccountStatusAsync(Guid accountId, UpdateAccountStatusRequest request, Guid adminUserId, string? ipAddress, CancellationToken cancellationToken = default);
}
