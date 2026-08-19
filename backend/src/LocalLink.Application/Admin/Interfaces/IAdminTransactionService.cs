using LocalLink.Application.Admin.DTOs;
using LocalLink.Application.Common.Models;
using LocalLink.Domain.Enums;

namespace LocalLink.Application.Admin.Interfaces;

public interface IAdminTransactionService
{
    Task<PagedResult<AdminTransactionListItemDto>> GetTransactionsAsync(
        int page,
        int pageSize,
        string? search = null,
        TransactionType? type = null,
        TransactionStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? accountNumber = null,
        CancellationToken cancellationToken = default);

    Task<AdminTransactionDetailDto> GetTransactionDetailAsync(Guid id, CancellationToken cancellationToken = default);
}
