using LocalLink.Application.Common.Models;
using LocalLink.Application.Payments.DTOs;

namespace LocalLink.Application.Payments.Interfaces;

public interface IPaymentService
{
    Task<PaymentReceiptDto> PayBillAsync(
        CreatePaymentRequest request,
        string? idempotencyKey,
        Guid currentUserId,
        string? ipAddress = null,
        CancellationToken cancellationToken = default);

    Task<PagedResult<PaymentListItemDto>> GetPaymentsAsync(
        Guid currentUserId,
        int page = 1,
        int pageSize = 10,
        string? status = null,
        DateOnly? fromDate = null,
        DateOnly? toDate = null,
        string? billType = null,
        CancellationToken cancellationToken = default);

    Task<PaymentDetailDto> GetPaymentDetailAsync(
        Guid paymentId,
        Guid currentUserId,
        CancellationToken cancellationToken = default);
}
