using LocalLink.Application.Bills.DTOs;
using LocalLink.Application.Bills.Interfaces;
using LocalLink.Application.Common.Exceptions;
using LocalLink.Application.Common.Models;
using LocalLink.Domain.Enums;
using LocalLink.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LocalLink.Infrastructure.Services;

public class BillService : IBillService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<BillService> _logger;

    public BillService(ApplicationDbContext context, ILogger<BillService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PagedResult<BillListItemDto>> GetBillsAsync(
        Guid currentUserId,
        int page = 1,
        int pageSize = 10,
        string? status = null,
        string? type = null,
        DateOnly? fromDueDate = null,
        DateOnly? toDueDate = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == currentUserId, cancellationToken);

        if (customer == null)
        {
            throw new NotFoundException("Customer profile not found.");
        }

        var query = _context.Bills
            .AsNoTracking()
            .Where(b => b.CustomerId == customer.Id);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<BillStatus>(status, true, out var billStatus))
        {
            query = query.Where(b => b.Status == billStatus);
        }

        if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<BillType>(type, true, out var billType))
        {
            query = query.Where(b => b.BillType == billType);
        }

        if (fromDueDate.HasValue)
        {
            query = query.Where(b => b.DueDate >= fromDueDate.Value);
        }

        if (toDueDate.HasValue)
        {
            query = query.Where(b => b.DueDate <= toDueDate.Value);
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(b => b.DueDate)
            .ThenByDescending(b => b.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new BillListItemDto
            {
                Id = b.Id,
                BillNumber = b.BillNumber,
                ProviderName = b.ProviderName,
                BillType = b.BillType.ToString().ToUpperInvariant(),
                Amount = b.Amount,
                DueDate = b.DueDate,
                Status = b.Status.ToString().ToUpperInvariant(),
                CreatedAtUtc = b.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<BillListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<BillDetailDto> GetBillDetailAsync(
        Guid billId,
        Guid currentUserId,
        CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == currentUserId, cancellationToken);

        if (customer == null)
        {
            throw new NotFoundException("Customer profile not found.");
        }

        var bill = await _context.Bills
            .Include(b => b.Payment)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == billId, cancellationToken);

        if (bill == null || bill.CustomerId != customer.Id)
        {
            throw new NotFoundException("Bill not found.");
        }

        return new BillDetailDto
        {
            Id = bill.Id,
            BillNumber = bill.BillNumber,
            ProviderName = bill.ProviderName,
            BillType = bill.BillType.ToString().ToUpperInvariant(),
            Amount = bill.Amount,
            DueDate = bill.DueDate,
            Status = bill.Status.ToString().ToUpperInvariant(),
            CreatedAtUtc = bill.CreatedAtUtc,
            UpdatedAtUtc = bill.UpdatedAtUtc,
            PaymentId = bill.Payment?.Id,
            PaidAtUtc = bill.Payment?.PaidAtUtc
        };
    }
}
