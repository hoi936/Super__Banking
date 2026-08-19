using LocalLink.Application.Bills.DTOs;
using LocalLink.Application.Common.Models;

namespace LocalLink.Application.Bills.Interfaces;

public interface IBillService
{
    Task<PagedResult<BillListItemDto>> GetBillsAsync(
        Guid currentUserId,
        int page = 1,
        int pageSize = 10,
        string? status = null,
        string? type = null,
        DateOnly? fromDueDate = null,
        DateOnly? toDueDate = null,
        CancellationToken cancellationToken = default);

    Task<BillDetailDto> GetBillDetailAsync(
        Guid billId,
        Guid currentUserId,
        CancellationToken cancellationToken = default);
}
