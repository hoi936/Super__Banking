using LocalLink.Application.Admin.DTOs;
using LocalLink.Application.Common.Models;
using LocalLink.Domain.Enums;

namespace LocalLink.Application.Admin.Interfaces;

public interface IAdminPaymentService
{
    Task<PagedResult<AdminPaymentListItemDto>> GetPaymentsAsync(
        int page,
        int pageSize,
        PaymentStatus? status = null,
        BillType? billType = null,
        string? reference = null,
        string? accountNumber = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    Task<AdminPaymentDetailDto> GetPaymentDetailAsync(Guid id, CancellationToken cancellationToken = default);
}
