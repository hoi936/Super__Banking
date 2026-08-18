using LocalLink.Application.Common.Exceptions;
using LocalLink.Application.Common.Models;
using LocalLink.Application.Transactions.DTOs;
using LocalLink.Application.Transactions.Interfaces;
using LocalLink.Domain.Enums;
using LocalLink.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LocalLink.Infrastructure.Services;

public class TransactionService : ITransactionService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(ApplicationDbContext context, ILogger<TransactionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PagedResult<TransactionListItemDto>> GetMyTransactionsAsync(
        Guid currentUserId,
        int page,
        int pageSize,
        Guid? accountId,
        string? type,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == currentUserId, cancellationToken);

        if (customer == null)
        {
            throw new NotFoundException("Customer", currentUserId);
        }

        var query = _context.Transactions
            .AsNoTracking()
            .Include(t => t.SourceAccount)
            .Include(t => t.DestinationAccount)
            .Where(t => (t.SourceAccount != null && t.SourceAccount.CustomerId == customer.Id)
                     || (t.DestinationAccount != null && t.DestinationAccount.CustomerId == customer.Id));

        if (accountId.HasValue)
        {
            var ownsAccount = await _context.BankAccounts
                .AnyAsync(ba => ba.Id == accountId.Value && ba.CustomerId == customer.Id, cancellationToken);

            if (!ownsAccount)
            {
                throw new NotFoundException("BankAccount", accountId.Value);
            }

            query = query.Where(t => t.SourceAccountId == accountId.Value || t.DestinationAccountId == accountId.Value);
        }

        if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<TransactionType>(type, true, out var parsedType))
        {
            query = query.Where(t => t.TransactionType == parsedType);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(t => t.CreatedAtUtc >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(t => t.CreatedAtUtc <= toDate.Value);
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var items = await query
            .OrderByDescending(t => t.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TransactionListItemDto
            {
                Id = t.Id,
                ReferenceNumber = t.ReferenceNumber,
                TransactionType = t.TransactionType.ToString().ToUpperInvariant(),
                SourceAccountId = t.SourceAccountId,
                SourceAccountNumber = t.SourceAccount != null ? t.SourceAccount.AccountNumber : null,
                DestinationAccountId = t.DestinationAccountId,
                DestinationAccountNumber = t.DestinationAccount != null ? t.DestinationAccount.AccountNumber : null,
                Amount = t.Amount,
                Currency = t.Currency,
                Description = t.Description,
                Status = t.Status.ToString().ToUpperInvariant(),
                CreatedAtUtc = t.CreatedAtUtc,
                CompletedAtUtc = t.CompletedAtUtc
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<TransactionListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<TransactionDetailDto> GetTransactionDetailAsync(
        Guid transactionId,
        Guid currentUserId,
        CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == currentUserId, cancellationToken);

        if (customer == null)
        {
            throw new NotFoundException("Customer", currentUserId);
        }

        var transaction = await _context.Transactions
            .AsNoTracking()
            .Include(t => t.SourceAccount)
            .Include(t => t.DestinationAccount)
            .FirstOrDefaultAsync(t => t.Id == transactionId, cancellationToken);

        if (transaction == null)
        {
            throw new NotFoundException("Transaction", transactionId);
        }

        bool isOwned = (transaction.SourceAccount != null && transaction.SourceAccount.CustomerId == customer.Id)
                    || (transaction.DestinationAccount != null && transaction.DestinationAccount.CustomerId == customer.Id);

        if (!isOwned)
        {
            throw new NotFoundException("Transaction", transactionId);
        }

        return new TransactionDetailDto
        {
            Id = transaction.Id,
            ReferenceNumber = transaction.ReferenceNumber,
            TransactionType = transaction.TransactionType.ToString().ToUpperInvariant(),
            SourceAccountId = transaction.SourceAccountId,
            SourceAccountNumber = transaction.SourceAccount?.AccountNumber,
            SourceAccountName = transaction.SourceAccount?.AccountName,
            DestinationAccountId = transaction.DestinationAccountId,
            DestinationAccountNumber = transaction.DestinationAccount?.AccountNumber,
            DestinationAccountName = transaction.DestinationAccount?.AccountName,
            Amount = transaction.Amount,
            Currency = transaction.Currency,
            Description = transaction.Description,
            Status = transaction.Status.ToString().ToUpperInvariant(),
            CreatedAtUtc = transaction.CreatedAtUtc,
            CompletedAtUtc = transaction.CompletedAtUtc
        };
    }
}
