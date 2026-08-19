using LocalLink.Application.PaymentGateway.DTOs;

namespace LocalLink.Application.PaymentGateway.Interfaces;

public interface IPaymentGatewayService
{
    Task<GatewayDepositReceiptDto> CreateDepositAsync(CreateGatewayDepositRequest request, Guid currentUserId, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task<bool> HandleCallbackAsync(GatewayCallbackRequest request, CancellationToken cancellationToken = default);
}
