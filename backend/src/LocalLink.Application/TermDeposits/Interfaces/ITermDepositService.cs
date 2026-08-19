using LocalLink.Application.TermDeposits.DTOs;

namespace LocalLink.Application.TermDeposits.Interfaces;

public interface ITermDepositService
{
    Task<IReadOnlyList<TermDepositDto>> GetMyTermDepositsAsync(
        Guid currentUserId,
        string? status = null,
        CancellationToken cancellationToken = default);

    Task<TermDepositDetailDto> GetMyTermDepositDetailAsync(
        Guid currentUserId,
        Guid termDepositId,
        CancellationToken cancellationToken = default);

    Task<TermDepositReceiptDto> OpenTermDepositAsync(
        CreateTermDepositRequest request,
        string? idempotencyKey,
        Guid currentUserId,
        string? ipAddress = null,
        CancellationToken cancellationToken = default);

    Task<int> MatureDueTermDepositsAsync(CancellationToken cancellationToken = default);
}
