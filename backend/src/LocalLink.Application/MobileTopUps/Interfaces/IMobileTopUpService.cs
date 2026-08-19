using LocalLink.Application.Common.Models;
using LocalLink.Application.MobileTopUps.DTOs;

namespace LocalLink.Application.MobileTopUps.Interfaces;

public interface IMobileTopUpService
{
    Task<IReadOnlyList<MobileProviderDto>> GetProvidersAsync(CancellationToken cancellationToken = default);

    Task<MobileTopUpReceiptDto> PurchaseAsync(
        CreateMobileTopUpRequest request,
        string? idempotencyKey,
        Guid currentUserId,
        string? ipAddress = null,
        CancellationToken cancellationToken = default);

    Task<PagedResult<MobileTopUpListItemDto>> GetMyPurchasesAsync(
        Guid currentUserId,
        int page = 1,
        int pageSize = 20,
        string? status = null,
        CancellationToken cancellationToken = default);
}
