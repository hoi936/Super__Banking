using LocalLink.Application.Common.Models;
using LocalLink.Application.Napas.DTOs;

namespace LocalLink.Application.Napas.Interfaces;

public interface INapasTransferService
{
    Task<IReadOnlyList<NapasBankDto>> GetBanksAsync(CancellationToken cancellationToken = default);
    Task<NapasLookupResultDto> LookupAsync(NapasLookupRequest request, CancellationToken cancellationToken = default);
    Task<NapasTransferReceiptDto> TransferAsync(CreateNapasTransferRequest request, string? idempotencyKey, Guid currentUserId, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task<PagedResult<NapasTransferListItemDto>> GetMyTransfersAsync(Guid currentUserId, int page = 1, int pageSize = 20, string? status = null, CancellationToken cancellationToken = default);
}
