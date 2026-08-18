using LocalLink.Application.Admin.DTOs;
using LocalLink.Application.Admin.Interfaces;
using LocalLink.Application.Common.Exceptions;
using LocalLink.Application.Common.Models;
using LocalLink.Domain.Enums;
using LocalLink.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LocalLink.Infrastructure.Services.Admin;

public class AdminTransactionService : IAdminTransactionService
{
    private readonly ApplicationDbContext _context;

    public AdminTransactionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<AdminTransactionListItemDto>> GetTransactionsAsync(
        int page,
        int pageSize,
        string? search = null,
        TransactionType? type = null,
        TransactionStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? accountNumber = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;
        if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
            throw new BadRequestException("FromDate cannot be greater than ToDate");

        var query = _context.Transactions.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(t => t.ReferenceNumber.Contains(search) || t.Description!.Contains(search));
        }

        if (type.HasValue) query = query.Where(t => t.TransactionType == type.Value);
        if (status.HasValue) query = query.Where(t => t.Status == status.Value);
        
        if (fromDate.HasValue) query = query.Where(t => t.CreatedAtUtc >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(t => t.CreatedAtUtc <= toDate.Value);

        if (!string.IsNullOrWhiteSpace(accountNumber))
        {
            query = query.Where(t => 
                (t.SourceAccountId != null && t.SourceAccount!.AccountNumber == accountNumber) ||
                (t.DestinationAccountId != null && t.DestinationAccount!.AccountNumber == accountNumber));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .Include(t => t.SourceAccount)
            .Include(t => t.DestinationAccount)
            .OrderByDescending(t => t.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new AdminTransactionListItemDto
            {
                Id = t.Id,
                ReferenceNumber = t.ReferenceNumber,
                TransactionType = t.TransactionType,
                Amount = t.Amount,
                Currency = t.Currency,
                Status = t.Status,
                SourceAccountNumber = t.SourceAccount != null ? t.SourceAccount.AccountNumber : null,
                DestinationAccountNumber = t.DestinationAccount != null ? t.DestinationAccount.AccountNumber : null,
                CreatedAtUtc = t.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<AdminTransactionListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<AdminTransactionDetailDto> GetTransactionDetailAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var txn = await _context.Transactions
            .AsNoTracking()
            .Include(t => t.SourceAccount).ThenInclude(a => a!.Customer)
            .Include(t => t.DestinationAccount).ThenInclude(a => a!.Customer)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (txn == null)
            throw new NotFoundException("Transaction not found");

        return new AdminTransactionDetailDto
        {
            Id = txn.Id,
            ReferenceNumber = txn.ReferenceNumber,
            TransactionType = txn.TransactionType,
            Amount = txn.Amount,
            Currency = txn.Currency,
            Status = txn.Status,
            Description = txn.Description,
            CreatedAtUtc = txn.CreatedAtUtc,
            CompletedAtUtc = txn.CompletedAtUtc,
            SourceAccount = txn.SourceAccount != null ? new AdminTransactionAccountDto
            {
                Id = txn.SourceAccount.Id,
                AccountNumber = txn.SourceAccount.AccountNumber,
                AccountName = txn.SourceAccount.AccountName,
                CustomerCode = txn.SourceAccount.Customer!.CustomerCode,
                CustomerFullName = txn.SourceAccount.Customer.FullName
            } : null,
            DestinationAccount = txn.DestinationAccount != null ? new AdminTransactionAccountDto
            {
                Id = txn.DestinationAccount.Id,
                AccountNumber = txn.DestinationAccount.AccountNumber,
                AccountName = txn.DestinationAccount.AccountName,
                CustomerCode = txn.DestinationAccount.Customer!.CustomerCode,
                CustomerFullName = txn.DestinationAccount.Customer.FullName
            } : null
        };
    }
}
