using LocalLink.Application.Common.Exceptions;
using LocalLink.Application.PaymentGateway.DTOs;
using LocalLink.Application.PaymentGateway.Interfaces;
using LocalLink.Domain.Entities;
using LocalLink.Domain.Enums;
using LocalLink.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LocalLink.Infrastructure.Services;

public class PaymentGatewayService : IPaymentGatewayService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PaymentGatewayService> _logger;

    public PaymentGatewayService(ApplicationDbContext context, ILogger<PaymentGatewayService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<GatewayDepositReceiptDto> CreateDepositAsync(CreateGatewayDepositRequest request, Guid currentUserId, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.UserId == currentUserId, cancellationToken);

        if (customer == null)
            throw new NotFoundException("Customer profile not found.");

        var account = await _context.BankAccounts
            .FirstOrDefaultAsync(a => a.Id == request.AccountId, cancellationToken);

        if (account == null || account.CustomerId != customer.Id)
            throw new NotFoundException("Account not found.");

        if (account.Status != AccountStatus.Active)
            throw new BadRequestException("Account is not active.");

        if (!Enum.TryParse<GatewayProvider>(request.Provider, true, out var provider))
            throw new BadRequestException("Invalid payment provider.");

        // Generate a reference number
        var referenceNumber = $"DEP{DateTime.UtcNow:yyyyMMddHHmmss}{new Random().Next(1000, 9999)}";

        var gatewayTransaction = new GatewayTransaction
        {
            Id = Guid.NewGuid(),
            AccountId = account.Id,
            Provider = provider,
            Amount = request.Amount,
            Currency = account.Currency,
            Status = GatewayTransactionStatus.Pending,
            ReferenceNumber = referenceNumber,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.GatewayTransactions.Add(gatewayTransaction);
        
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = currentUserId,
            Action = "DEPOSIT_INITIATED",
            EntityType = "GatewayTransaction",
            EntityId = gatewayTransaction.Id.ToString(),
            Description = $"Initiated deposit of {request.Amount:N2} {account.Currency} via {provider}. Ref: {referenceNumber}",
            IpAddress = ipAddress,
            CreatedAtUtc = DateTime.UtcNow
        };
        _context.AuditLogs.Add(auditLog);

        await _context.SaveChangesAsync(cancellationToken);

        // Simulate payment URL generation
        var callbackUrl = $"http://localhost:3000/deposit/callback?ref={referenceNumber}&provider={provider}&amount={request.Amount}";
        
        return new GatewayDepositReceiptDto
        {
            TransactionId = gatewayTransaction.Id,
            ReferenceNumber = referenceNumber,
            Provider = provider.ToString(),
            Amount = gatewayTransaction.Amount,
            Status = GatewayTransactionStatus.Pending.ToString(),
            PaymentUrl = callbackUrl,
            CreatedAtUtc = gatewayTransaction.CreatedAtUtc
        };
    }

    public async Task<bool> HandleCallbackAsync(GatewayCallbackRequest request, CancellationToken cancellationToken = default)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var dbTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var gatewayTx = await _context.GatewayTransactions
                    .Include(t => t.Account)
                    .ThenInclude(a => a!.Customer)
                    .FirstOrDefaultAsync(t => t.ReferenceNumber == request.ReferenceNumber, cancellationToken);

                if (gatewayTx == null)
                {
                    _logger.LogWarning("Callback received for unknown reference: {Ref}", request.ReferenceNumber);
                    return false; // Return false or throw? Let's just return false
                }

                if (gatewayTx.Status != GatewayTransactionStatus.Pending)
                {
                    _logger.LogInformation("Callback for already processed transaction: {Ref}", request.ReferenceNumber);
                    return true; // Already processed
                }

                if (request.Status.Equals("Success", StringComparison.OrdinalIgnoreCase))
                {
                    gatewayTx.Status = GatewayTransactionStatus.Success;
                    gatewayTx.CompletedAtUtc = DateTime.UtcNow;
                    gatewayTx.GatewayTransactionId = request.GatewayTransactionId;

                    if (gatewayTx.Account != null)
                    {
                        // Credit Account
                        gatewayTx.Account.Balance += gatewayTx.Amount;
                        gatewayTx.Account.UpdatedAtUtc = DateTime.UtcNow;

                        // Create Ledger Entry
                        var transactionEntity = new Transaction
                        {
                            Id = Guid.NewGuid(),
                            ReferenceNumber = gatewayTx.ReferenceNumber,
                            TransactionType = TransactionType.Deposit,
                            SourceAccountId = null, // External source
                            DestinationAccountId = gatewayTx.AccountId,
                            Amount = gatewayTx.Amount,
                            Currency = gatewayTx.Currency,
                            Description = $"Deposit via {gatewayTx.Provider}",
                            Status = TransactionStatus.Completed,
                            CreatedAtUtc = DateTime.UtcNow,
                            CompletedAtUtc = DateTime.UtcNow
                        };
                        _context.Transactions.Add(transactionEntity);

                        // Notify Customer
                        if (gatewayTx.Account.Customer != null)
                        {
                            var notification = new Notification
                            {
                                Id = Guid.NewGuid(),
                                UserId = gatewayTx.Account.Customer.UserId,
                                Title = "Nạp tiền thành công",
                                Message = $"Tài khoản {gatewayTx.Account.AccountNumber} vừa được cộng {gatewayTx.Amount:N0} {gatewayTx.Currency} từ {gatewayTx.Provider}.",
                                Type = NotificationType.System,
                                IsRead = false,
                                CreatedAtUtc = DateTime.UtcNow
                            };
                            _context.Notifications.Add(notification);
                        }
                    }
                }
                else
                {
                    gatewayTx.Status = GatewayTransactionStatus.Failed;
                    gatewayTx.CompletedAtUtc = DateTime.UtcNow;
                    gatewayTx.GatewayTransactionId = request.GatewayTransactionId;
                }

                await _context.SaveChangesAsync(cancellationToken);
                await dbTransaction.CommitAsync(cancellationToken);
                
                _logger.LogInformation("Callback processed successfully for {Ref}, Status: {Status}", request.ReferenceNumber, request.Status);
                return true;
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error processing callback for {Ref}", request.ReferenceNumber);
                throw;
            }
        });
    }
}
