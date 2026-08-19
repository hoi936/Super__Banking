using LocalLink.Application.Admin.DTOs;
using LocalLink.Application.Admin.Interfaces;
using LocalLink.Application.Common.Exceptions;
using LocalLink.Application.Common.Models;
using LocalLink.Domain.Enums;
using LocalLink.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LocalLink.Infrastructure.Services.Admin;

public class AdminPaymentService : IAdminPaymentService
{
    private readonly ApplicationDbContext _context;

    public AdminPaymentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<AdminPaymentListItemDto>> GetPaymentsAsync(
        int page,
        int pageSize,
        PaymentStatus? status = null,
        BillType? billType = null,
        string? reference = null,
        string? accountNumber = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;
        if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
            throw new BadRequestException("FromDate cannot be greater than ToDate");

        var query = _context.Payments
            .Include(p => p.Bill)
            .Include(p => p.Account)
            .Include(p => p.Transaction)
            .AsNoTracking().AsQueryable();

        if (status.HasValue) query = query.Where(p => p.Status == status.Value);
        if (billType.HasValue) query = query.Where(p => p.Bill!.BillType == billType.Value);
        if (!string.IsNullOrWhiteSpace(reference)) query = query.Where(p => p.Transaction!.ReferenceNumber.Contains(reference));
        if (!string.IsNullOrWhiteSpace(accountNumber)) query = query.Where(p => p.Account!.AccountNumber == accountNumber);
        
        if (fromDate.HasValue) query = query.Where(p => p.CreatedAtUtc >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(p => p.CreatedAtUtc <= toDate.Value);

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(p => p.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new AdminPaymentListItemDto
            {
                Id = p.Id,
                PaymentReference = p.Transaction!.ReferenceNumber,
                Amount = p.Amount,
                Currency = p.Transaction.Currency,
                Status = p.Status,
                BillNumber = p.Bill!.BillNumber,
                BillType = p.Bill.BillType,
                AccountNumber = p.Account!.AccountNumber,
                CreatedAtUtc = p.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<AdminPaymentListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<AdminPaymentDetailDto> GetPaymentDetailAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var payment = await _context.Payments
            .AsNoTracking()
            .Include(p => p.Bill)
            .Include(p => p.Account).ThenInclude(a => a!.Customer)
            .Include(p => p.Transaction)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (payment == null)
            throw new NotFoundException("Payment not found");

        return new AdminPaymentDetailDto
        {
            Id = payment.Id,
            PaymentReference = payment.Transaction!.ReferenceNumber,
            Amount = payment.Amount,
            Currency = payment.Transaction.Currency,
            Status = payment.Status,
            CustomerFullName = payment.Account!.Customer!.FullName,
            CustomerCode = payment.Account.Customer.CustomerCode,
            AccountNumber = payment.Account.AccountNumber,
            BillNumber = payment.Bill!.BillNumber,
            BillType = payment.Bill.BillType,
            ProviderName = payment.Bill.ProviderName,
            TransactionReference = payment.Transaction?.ReferenceNumber,
            IdempotencyKey = payment.IdempotencyKey,
            CreatedAtUtc = payment.CreatedAtUtc,
            PaidAtUtc = payment.PaidAtUtc
        };
    }
}
