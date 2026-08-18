using System.Security.Cryptography;
using LocalLink.Application.Common.Exceptions;
using LocalLink.Application.Common.Models;
using LocalLink.Application.Payments.DTOs;
using LocalLink.Application.Payments.Interfaces;
using LocalLink.Domain.Entities;
using LocalLink.Domain.Enums;
using LocalLink.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LocalLink.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(ApplicationDbContext context, ILogger<PaymentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaymentReceiptDto> PayBillAsync(
        CreatePaymentRequest request,
        string? idempotencyKey,
        Guid currentUserId,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.UserId == currentUserId, cancellationToken);

        if (customer == null)
        {
            throw new NotFoundException("Customer profile not found.");
        }

        // Idempotency Check
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            var trimmedKey = idempotencyKey.Trim();
            var existingPayment = await _context.Payments
                .Include(p => p.Bill)
                .Include(p => p.Account)
                .Include(p => p.Transaction)
                .FirstOrDefaultAsync(p => p.IdempotencyKey == trimmedKey, cancellationToken);

            if (existingPayment != null)
            {
                // Verify payload matches
                if (existingPayment.BillId == request.BillId && existingPayment.AccountId == request.AccountId)
                {
                    _logger.LogInformation("Returning idempotent replay receipt for Payment {PaymentId} with key {Key}", existingPayment.Id, trimmedKey);
                    return new PaymentReceiptDto
                    {
                        PaymentId = existingPayment.Id,
                        Reference = existingPayment.Transaction?.ReferenceNumber ?? string.Empty,
                        BillNumber = existingPayment.Bill?.BillNumber ?? string.Empty,
                        ProviderName = existingPayment.Bill?.ProviderName ?? string.Empty,
                        AccountNumber = existingPayment.Account?.AccountNumber ?? string.Empty,
                        Amount = existingPayment.Amount,
                        Currency = existingPayment.Account?.Currency ?? "VND",
                        Status = existingPayment.Status.ToString().ToUpperInvariant(),
                        PaidAtUtc = existingPayment.PaidAtUtc
                    };
                }

                _logger.LogWarning("Idempotency key {Key} re-used with different payload.", trimmedKey);
                throw new ConflictException("Idempotency key was previously used with a different payment payload.");
            }
        }

        // Load and validate Bill
        var bill = await _context.Bills
            .FirstOrDefaultAsync(b => b.Id == request.BillId, cancellationToken);

        if (bill == null || bill.CustomerId != customer.Id)
        {
            throw new NotFoundException("Bill not found.");
        }

        if (bill.Status == BillStatus.Paid)
        {
            throw new BadRequestException("Bill has already been paid.");
        }

        if (bill.Status == BillStatus.Cancelled)
        {
            throw new BadRequestException("Cancelled bills cannot be paid.");
        }

        // Load and validate Source Account
        var account = await _context.BankAccounts
            .FirstOrDefaultAsync(a => a.Id == request.AccountId, cancellationToken);

        if (account == null || account.CustomerId != customer.Id)
        {
            throw new NotFoundException("Source bank account not found.");
        }

        if (account.Status != AccountStatus.Active)
        {
            throw new BadRequestException($"Source bank account is {account.Status.ToString().ToUpperInvariant()} and cannot be used for payments.");
        }

        if (account.Currency != "VND")
        {
            throw new BadRequestException("Cross-currency bill payments are not supported.");
        }

        if (account.Balance < bill.Amount)
        {
            throw new BadRequestException("Insufficient funds in account to complete bill payment.");
        }

        // Atomic Transaction Execution with SQL Server Execution Strategy
        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var dbTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // 1. Debit Account
                account.Balance -= bill.Amount;
                account.UpdatedAtUtc = DateTime.UtcNow;

                // 2. Generate Reference
                var referenceNumber = $"PAY{DateTime.UtcNow:yyyyMMddHHmmss}{RandomNumberGenerator.GetInt32(1000, 9999)}";

                // 3. Create Transaction Ledger Record
                var transactionEntity = new Transaction
                {
                    Id = Guid.NewGuid(),
                    ReferenceNumber = referenceNumber,
                    TransactionType = TransactionType.Payment,
                    SourceAccountId = account.Id,
                    DestinationAccountId = null,
                    Amount = bill.Amount,
                    Currency = account.Currency,
                    Description = $"Payment for {bill.ProviderName} ({bill.BillNumber})",
                    Status = TransactionStatus.Completed,
                    CreatedAtUtc = DateTime.UtcNow,
                    CompletedAtUtc = DateTime.UtcNow
                };
                _context.Transactions.Add(transactionEntity);

                // 4. Create Payment Record
                var paymentEntity = new Payment
                {
                    Id = Guid.NewGuid(),
                    BillId = bill.Id,
                    AccountId = account.Id,
                    TransactionId = transactionEntity.Id,
                    Amount = bill.Amount,
                    Status = PaymentStatus.Completed,
                    PaidAtUtc = DateTime.UtcNow,
                    IdempotencyKey = string.IsNullOrWhiteSpace(idempotencyKey) ? null : idempotencyKey.Trim(),
                    CreatedAtUtc = DateTime.UtcNow
                };
                _context.Payments.Add(paymentEntity);

                // 5. Update Bill Status
                bill.Status = BillStatus.Paid;
                bill.UpdatedAtUtc = DateTime.UtcNow;

                // 6. Create Audit Log
                var auditLog = new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = currentUserId,
                    Action = "PAYMENT_COMPLETED",
                    EntityType = "Payment",
                    EntityId = paymentEntity.Id.ToString(),
                    Description = $"Paid {bill.Amount:N2} {account.Currency} for {bill.ProviderName} ({bill.BillNumber}) from account {account.AccountNumber}. Reference: {referenceNumber}",
                    IpAddress = ipAddress,
                    CreatedAtUtc = DateTime.UtcNow
                };
                _context.AuditLogs.Add(auditLog);

                // 7. Create Notification
                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = currentUserId,
                    Title = "Thanh toán thành công",
                    Message = $"Bạn đã thanh toán hóa đơn {bill.ProviderName} ({bill.BillNumber}) số tiền {bill.Amount:N0} {account.Currency} thành công.",
                    Type = NotificationType.Payment,
                    IsRead = false,
                    CreatedAtUtc = DateTime.UtcNow
                };
                _context.Notifications.Add(notification);

                await _context.SaveChangesAsync(cancellationToken);
                await dbTransaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Payment {Ref} completed for Bill {BillNum}: {Amount} {Currency} from Account {AccNum}",
                    referenceNumber, bill.BillNumber, bill.Amount, account.Currency, account.AccountNumber);

                return new PaymentReceiptDto
                {
                    PaymentId = paymentEntity.Id,
                    Reference = referenceNumber,
                    BillNumber = bill.BillNumber,
                    ProviderName = bill.ProviderName,
                    AccountNumber = account.AccountNumber,
                    Amount = bill.Amount,
                    Currency = account.Currency,
                    Status = "COMPLETED",
                    PaidAtUtc = paymentEntity.PaidAtUtc
                };
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await dbTransaction.RollbackAsync(cancellationToken);
                _logger.LogWarning(ex, "Concurrency conflict during payment for Account {AccountId}", account.Id);
                throw new ConflictException("A concurrency conflict occurred while processing the payment. Please refresh your balance and try again.");
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Unexpected error during payment for bill {BillId}", bill.Id);
                throw;
            }
        });
    }

    public async Task<PagedResult<PaymentListItemDto>> GetPaymentsAsync(
        Guid currentUserId,
        int page = 1,
        int pageSize = 10,
        string? status = null,
        DateOnly? fromDate = null,
        DateOnly? toDate = null,
        string? billType = null,
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

        var query = _context.Payments
            .Include(p => p.Bill)
            .Include(p => p.Account)
            .Include(p => p.Transaction)
            .AsNoTracking()
            .Where(p => p.Bill.CustomerId == customer.Id);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PaymentStatus>(status, true, out var payStatus))
        {
            query = query.Where(p => p.Status == payStatus);
        }

        if (!string.IsNullOrWhiteSpace(billType) && Enum.TryParse<BillType>(billType, true, out var bType))
        {
            query = query.Where(p => p.Bill.BillType == bType);
        }

        if (fromDate.HasValue)
        {
            var fromDateTime = fromDate.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(p => (p.PaidAtUtc ?? p.CreatedAtUtc) >= fromDateTime);
        }

        if (toDate.HasValue)
        {
            var toDateTime = toDate.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
            query = query.Where(p => (p.PaidAtUtc ?? p.CreatedAtUtc) <= toDateTime);
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(p => p.PaidAtUtc ?? p.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PaymentListItemDto
            {
                Id = p.Id,
                ReferenceNumber = p.Transaction != null ? p.Transaction.ReferenceNumber : string.Empty,
                BillNumber = p.Bill != null ? p.Bill.BillNumber : string.Empty,
                ProviderName = p.Bill != null ? p.Bill.ProviderName : string.Empty,
                BillType = p.Bill != null ? p.Bill.BillType.ToString().ToUpperInvariant() : string.Empty,
                AccountNumber = p.Account != null ? p.Account.AccountNumber : string.Empty,
                Amount = p.Amount,
                Currency = p.Account != null ? p.Account.Currency : "VND",
                Status = p.Status.ToString().ToUpperInvariant(),
                PaidAtUtc = p.PaidAtUtc,
                CreatedAtUtc = p.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<PaymentListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<PaymentDetailDto> GetPaymentDetailAsync(
        Guid paymentId,
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

        var payment = await _context.Payments
            .Include(p => p.Bill)
            .Include(p => p.Account)
            .Include(p => p.Transaction)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken);

        if (payment == null || payment.Bill == null || payment.Bill.CustomerId != customer.Id)
        {
            throw new NotFoundException("Payment record not found.");
        }

        return new PaymentDetailDto
        {
            Id = payment.Id,
            ReferenceNumber = payment.Transaction?.ReferenceNumber ?? string.Empty,
            BillId = payment.BillId,
            BillNumber = payment.Bill.BillNumber,
            ProviderName = payment.Bill.ProviderName,
            BillType = payment.Bill.BillType.ToString().ToUpperInvariant(),
            AccountId = payment.AccountId,
            AccountNumber = payment.Account?.AccountNumber ?? string.Empty,
            Amount = payment.Amount,
            Currency = payment.Account?.Currency ?? "VND",
            Status = payment.Status.ToString().ToUpperInvariant(),
            PaidAtUtc = payment.PaidAtUtc,
            CreatedAtUtc = payment.CreatedAtUtc,
            IdempotencyKey = payment.IdempotencyKey
        };
    }
}
