using LocalLink.Application.Common.Models;
using LocalLink.Application.Transactions.DTOs;

namespace LocalLink.Application.Transactions.Interfaces;

public interface ITransactionService
{
    Task<PagedResult<TransactionListItemDto>> GetMyTransactionsAsync(Guid currentUserId, int page, int pageSize, Guid? accountId, string? type, DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default);
    Task<TransactionDetailDto> GetTransactionDetailAsync(Guid transactionId, Guid currentUserId, CancellationToken cancellationToken = default);
}
