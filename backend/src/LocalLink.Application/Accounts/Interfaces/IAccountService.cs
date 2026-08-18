using LocalLink.Application.Accounts.DTOs;

namespace LocalLink.Application.Accounts.Interfaces;

public interface IAccountService
{
    Task<IReadOnlyList<AccountSummaryDto>> GetMyAccountsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<AccountDetailDto> GetMyAccountDetailAsync(Guid userId, Guid accountId, CancellationToken cancellationToken = default);
    Task<AccountLookupDto> LookupAccountAsync(string accountNumber, CancellationToken cancellationToken = default);
}
