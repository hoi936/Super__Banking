using System.Security.Cryptography;
using LocalLink.Application.Common.Exceptions;
using LocalLink.Application.Common.Models;
using LocalLink.Application.Transfers.DTOs;
using LocalLink.Application.Transfers.Interfaces;
using LocalLink.Domain.Entities;
using LocalLink.Domain.Enums;
using LocalLink.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LocalLink.Infrastructure.Services;

public class TransferService : ITransferService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TransferService> _logger;

    public TransferService(ApplicationDbContext context, ILogger<TransferService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TransferReceiptDto> TransferAsync(
        CreateTransferRequest request,
        string? idempotencyKey,
        Guid currentUserId,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        if (request.Amount <= 0)
        {
            throw new BadRequestException("Transfer amount must be greater than zero.");
        }

        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == currentUserId, cancellationToken);

        if (customer == null)
        {
            throw new NotFoundException("Customer", currentUserId);
        }

        // Idempotency check
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            var cleanKey = idempotencyKey.Trim();
            var existingTransfer = await _context.Transfers
                .AsNoTracking()
                .Include(tr => tr.Transaction)
                .Include(tr => tr.SourceAccount)
                .Include(tr => tr.DestinationAccount)
                    .ThenInclude(da => da.Customer)
                .FirstOrDefaultAsync(tr => tr.IdempotencyKey == cleanKey, cancellationToken);

            if (existingTransfer != null)
            {
                if (existingTransfer.SourceAccount.CustomerId != customer.Id)
                {
                    throw new ForbiddenException("Access denied to the specified idempotency key.");
                }

                bool isSamePayload = existingTransfer.SourceAccountId == request.SourceAccountId
                    && string.Equals(existingTransfer.DestinationAccount.AccountNumber, request.DestinationAccountNumber.Trim(), StringComparison.OrdinalIgnoreCase)
                    && existingTransfer.Amount == request.Amount;

                if (isSamePayload)
                {
                    _logger.LogInformation("Idempotent transfer request replayed for key: {Key}", cleanKey);
                    return new TransferReceiptDto
                    {
                        TransferId = existingTransfer.Id,
                        Reference = existingTransfer.Transaction?.ReferenceNumber ?? string.Empty,
                        SourceAccountId = existingTransfer.SourceAccountId,
                        SourceAccountNumber = existingTransfer.SourceAccount.AccountNumber,
                        DestinationAccountId = existingTransfer.DestinationAccountId,
                        DestinationAccountNumber = existingTransfer.DestinationAccount.AccountNumber,
                        DestinationAccountName = existingTransfer.DestinationAccount.Customer?.FullName ?? existingTransfer.DestinationAccount.AccountName,
                        Amount = existingTransfer.Amount,
                        Currency = existingTransfer.SourceAccount.Currency,
                        Description = existingTransfer.Description,
                        Status = existingTransfer.Status.ToString().ToUpperInvariant(),
                        CreatedAtUtc = existingTransfer.CreatedAtUtc,
                        CompletedAtUtc = existingTransfer.CompletedAtUtc
                    };
                }

                throw new ConflictException("Idempotency key was previously used with a different transfer payload.");
            }
        }

        // Validate Source Account
        var sourceAccount = await _context.BankAccounts
            .FirstOrDefaultAsync(ba => ba.Id == request.SourceAccountId, cancellationToken);

        if (sourceAccount == null || sourceAccount.CustomerId != customer.Id)
        {
            throw new NotFoundException("Source bank account not found or not owned by the current customer.");
        }

        if (sourceAccount.Status != AccountStatus.Active)
        {
            throw new BadRequestException($"Source account is {sourceAccount.Status} and cannot perform transfers.");
        }

        if (sourceAccount.Balance < request.Amount)
        {
            throw new BadRequestException("Insufficient funds.");
        }

        // Validate Destination Account
        var destAccountNumber = request.DestinationAccountNumber.Trim();
        var destinationAccount = await _context.BankAccounts
            .Include(ba => ba.Customer)
            .FirstOrDefaultAsync(ba => ba.AccountNumber == destAccountNumber, cancellationToken);

        if (destinationAccount == null)
        {
            throw new NotFoundException($"Destination account '{destAccountNumber}' was not found.");
        }

        if (destinationAccount.Id == sourceAccount.Id)
        {
            throw new BadRequestException("Cannot transfer money to the same bank account.");
        }

        if (destinationAccount.Status != AccountStatus.Active)
        {
            throw new BadRequestException($"Destination account is {destinationAccount.Status} and cannot receive transfers.");
        }

        if (!string.Equals(sourceAccount.Currency, destinationAccount.Currency, StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestException("Cross-currency transfers are not supported.");
        }

        // Atomic Transaction Execution with SQL Server Execution Strategy
        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var dbTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // 1. Debit Source
                sourceAccount.Balance -= request.Amount;
                sourceAccount.UpdatedAtUtc = DateTime.UtcNow;

                // 2. Credit Destination
                destinationAccount.Balance += request.Amount;
                destinationAccount.UpdatedAtUtc = DateTime.UtcNow;

                // 3. Generate Reference
                var referenceNumber = $"TRF{DateTime.UtcNow:yyyyMMddHHmmss}{RandomNumberGenerator.GetInt32(1000, 9999)}";

                // 4. Create Transaction Ledger Record
                var transactionEntity = new Transaction
                {
                    Id = Guid.NewGuid(),
                    ReferenceNumber = referenceNumber,
                    TransactionType = TransactionType.Transfer,
                    SourceAccountId = sourceAccount.Id,
                    DestinationAccountId = destinationAccount.Id,
                    Amount = request.Amount,
                    Currency = sourceAccount.Currency,
                    Description = request.Description ?? "Bank transfer",
                    Status = TransactionStatus.Completed,
                    CreatedAtUtc = DateTime.UtcNow,
                    CompletedAtUtc = DateTime.UtcNow
                };
                _context.Transactions.Add(transactionEntity);

                // 5. Create Transfer Record
                var transferEntity = new Transfer
                {
                    Id = Guid.NewGuid(),
                    TransactionId = transactionEntity.Id,
                    SourceAccountId = sourceAccount.Id,
                    DestinationAccountId = destinationAccount.Id,
                    Amount = request.Amount,
                    Description = request.Description,
                    Status = TransferStatus.Completed,
                    IdempotencyKey = string.IsNullOrWhiteSpace(idempotencyKey) ? null : idempotencyKey.Trim(),
                    CreatedAtUtc = DateTime.UtcNow,
                    CompletedAtUtc = DateTime.UtcNow
                };
                _context.Transfers.Add(transferEntity);

                // 6. Create Audit Log
                var auditLog = new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = currentUserId,
                    Action = "TRANSFER_COMPLETED",
                    EntityType = "Transfer",
                    EntityId = transferEntity.Id.ToString(),
                    Description = $"Transferred {request.Amount:N2} {sourceAccount.Currency} from {sourceAccount.AccountNumber} to {destinationAccount.AccountNumber}. Reference: {referenceNumber}",
                    IpAddress = ipAddress,
                    CreatedAtUtc = DateTime.UtcNow
                };
                _context.AuditLogs.Add(auditLog);

                // 7. Create Notifications
                var senderNotification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = currentUserId,
                    Title = "Chuyển tiền thành công",
                    Message = $"Bạn đã chuyển {request.Amount:N0} {sourceAccount.Currency} đến {destinationAccount.Customer?.FullName ?? destinationAccount.AccountNumber} ({destinationAccount.AccountNumber}).",
                    Type = NotificationType.Transfer,
                    IsRead = false,
                    CreatedAtUtc = DateTime.UtcNow
                };
                _context.Notifications.Add(senderNotification);

                if (destinationAccount.Customer != null)
                {
                    var receiverNotification = new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = destinationAccount.Customer.UserId,
                        Title = "Nhận tiền thành công",
                        Message = $"Tài khoản {destinationAccount.AccountNumber} đã nhận {request.Amount:N0} {destinationAccount.Currency} từ {sourceAccount.AccountNumber}.",
                        Type = NotificationType.Transfer,
                        IsRead = false,
                        CreatedAtUtc = DateTime.UtcNow
                    };
                    _context.Notifications.Add(receiverNotification);
                }

                await _context.SaveChangesAsync(cancellationToken);
                await dbTransaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Transfer {Ref} executed successfully: {Amount} {Currency} from {Src} to {Dst}",
                    referenceNumber, request.Amount, sourceAccount.Currency, sourceAccount.AccountNumber, destinationAccount.AccountNumber);

                return new TransferReceiptDto
                {
                    TransferId = transferEntity.Id,
                    Reference = referenceNumber,
                    SourceAccountId = sourceAccount.Id,
                    SourceAccountNumber = sourceAccount.AccountNumber,
                    DestinationAccountId = destinationAccount.Id,
                    DestinationAccountNumber = destinationAccount.AccountNumber,
                    DestinationAccountName = destinationAccount.Customer?.FullName ?? destinationAccount.AccountName,
                    Amount = request.Amount,
                    Currency = sourceAccount.Currency,
                    Description = request.Description,
                    Status = "COMPLETED",
                    CreatedAtUtc = transferEntity.CreatedAtUtc,
                    CompletedAtUtc = transferEntity.CompletedAtUtc
                };
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await dbTransaction.RollbackAsync(cancellationToken);
                _logger.LogWarning(ex, "Concurrency conflict during transfer for Account {AccountId}", sourceAccount.Id);
                throw new ConflictException("A concurrency conflict occurred while processing the transfer. Please refresh your balance and try again.");
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Unexpected error during transfer from {Source} to {Destination}", sourceAccount.AccountNumber, destinationAccount.AccountNumber);
                throw;
            }
        });
    }

    public async Task<TransferReceiptDto> GetTransferDetailAsync(Guid transferId, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == currentUserId, cancellationToken);

        if (customer == null)
        {
            throw new NotFoundException("Customer", currentUserId);
        }

        var transfer = await _context.Transfers
            .AsNoTracking()
            .Include(tr => tr.Transaction)
            .Include(tr => tr.SourceAccount)
            .Include(tr => tr.DestinationAccount)
                .ThenInclude(da => da.Customer)
            .FirstOrDefaultAsync(tr => tr.Id == transferId, cancellationToken);

        if (transfer == null)
        {
            throw new NotFoundException("Transfer", transferId);
        }

        // Ownership: Current customer must own either the source or the destination account
        bool isOwned = transfer.SourceAccount.CustomerId == customer.Id || transfer.DestinationAccount.CustomerId == customer.Id;
        if (!isOwned)
        {
            throw new NotFoundException("Transfer", transferId);
        }

        return new TransferReceiptDto
        {
            TransferId = transfer.Id,
            Reference = transfer.Transaction?.ReferenceNumber ?? string.Empty,
            SourceAccountId = transfer.SourceAccountId,
            SourceAccountNumber = transfer.SourceAccount.AccountNumber,
            DestinationAccountId = transfer.DestinationAccountId,
            DestinationAccountNumber = transfer.DestinationAccount.AccountNumber,
            DestinationAccountName = transfer.DestinationAccount.Customer?.FullName ?? transfer.DestinationAccount.AccountName,
            Amount = transfer.Amount,
            Currency = transfer.SourceAccount.Currency,
            Description = transfer.Description,
            Status = transfer.Status.ToString().ToUpperInvariant(),
            CreatedAtUtc = transfer.CreatedAtUtc,
            CompletedAtUtc = transfer.CompletedAtUtc
        };
    }

    public async Task<PagedResult<TransferListItemDto>> GetMyTransfersAsync(
        Guid currentUserId,
        int page,
        int pageSize,
        string? status,
        DateTime? fromDate,
        DateTime? toDate,
        Guid? accountId,
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

        var query = _context.Transfers
            .AsNoTracking()
            .Include(tr => tr.Transaction)
            .Include(tr => tr.SourceAccount)
            .Include(tr => tr.DestinationAccount)
                .ThenInclude(da => da.Customer)
            .Where(tr => tr.SourceAccount.CustomerId == customer.Id || tr.DestinationAccount.CustomerId == customer.Id);

        if (accountId.HasValue)
        {
            var ownsAccount = await _context.BankAccounts
                .AnyAsync(ba => ba.Id == accountId.Value && ba.CustomerId == customer.Id, cancellationToken);

            if (!ownsAccount)
            {
                throw new NotFoundException("BankAccount", accountId.Value);
            }

            query = query.Where(tr => tr.SourceAccountId == accountId.Value || tr.DestinationAccountId == accountId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TransferStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(tr => tr.Status == parsedStatus);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(tr => tr.CreatedAtUtc >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(tr => tr.CreatedAtUtc <= toDate.Value);
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var items = await query
            .OrderByDescending(tr => tr.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(tr => new TransferListItemDto
            {
                Id = tr.Id,
                Reference = tr.Transaction.ReferenceNumber,
                SourceAccountId = tr.SourceAccountId,
                SourceAccountNumber = tr.SourceAccount.AccountNumber,
                DestinationAccountId = tr.DestinationAccountId,
                DestinationAccountNumber = tr.DestinationAccount.AccountNumber,
                DestinationAccountName = tr.DestinationAccount.Customer != null ? tr.DestinationAccount.Customer.FullName : tr.DestinationAccount.AccountName,
                Amount = tr.Amount,
                Description = tr.Description ?? string.Empty,
                CreatedAtUtc = tr.CreatedAtUtc,
                CompletedAtUtc = tr.CompletedAtUtc
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<TransferListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }
}
