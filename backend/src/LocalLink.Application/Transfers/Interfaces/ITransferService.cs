using LocalLink.Application.Common.Models;
using LocalLink.Application.Transfers.DTOs;

namespace LocalLink.Application.Transfers.Interfaces;

public interface ITransferService
{
    Task<TransferReceiptDto> TransferAsync(CreateTransferRequest request, string? idempotencyKey, Guid currentUserId, string? ipAddress, CancellationToken cancellationToken = default);
    Task<TransferReceiptDto> GetTransferDetailAsync(Guid transferId, Guid currentUserId, CancellationToken cancellationToken = default);
    Task<PagedResult<TransferListItemDto>> GetMyTransfersAsync(Guid currentUserId, int page, int pageSize, string? status, DateTime? fromDate, DateTime? toDate, Guid? accountId, CancellationToken cancellationToken = default);
}
