using LocalLink.Application.Accounts.DTOs;
using LocalLink.Application.Accounts.Interfaces;
using LocalLink.Application.Common.Exceptions;
using LocalLink.Domain.Entities;
using LocalLink.Domain.Enums;
using LocalLink.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LocalLink.Infrastructure.Services;

public class AdminAccountService : IAdminAccountService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AdminAccountService> _logger;

    public AdminAccountService(ApplicationDbContext context, ILogger<AdminAccountService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<AccountSummaryDto>> GetCustomerAccountsAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var customerExists = await _context.Customers
            .AsNoTracking()
            .AnyAsync(c => c.Id == customerId, cancellationToken);

        if (!customerExists)
        {
            throw new NotFoundException("Customer", customerId);
        }

        var accounts = await _context.BankAccounts
            .AsNoTracking()
            .Where(ba => ba.CustomerId == customerId)
            .OrderBy(ba => ba.CreatedAtUtc)
            .Select(ba => new AccountSummaryDto
            {
                Id = ba.Id,
                AccountNumber = ba.AccountNumber,
                AccountName = ba.AccountName,
                AccountType = ba.AccountType.ToString().ToUpperInvariant(),
                Balance = ba.Balance,
                Currency = ba.Currency,
                Status = ba.Status.ToString().ToUpperInvariant()
            })
            .ToListAsync(cancellationToken);

        return accounts;
    }

    public async Task<AccountSummaryDto> UpdateAccountStatusAsync(
        Guid accountId, 
        UpdateAccountStatusRequest request, 
        Guid adminUserId, 
        string? ipAddress, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Status))
        {
            throw new BadRequestException("Status is required.");
        }

        if (!Enum.TryParse<AccountStatus>(request.Status, true, out var newStatus))
        {
            throw new BadRequestException($"Invalid account status: '{request.Status}'. Allowed values: ACTIVE, LOCKED.");
        }

        if (newStatus != AccountStatus.Active && newStatus != AccountStatus.Locked)
        {
            throw new BadRequestException("Only ACTIVE and LOCKED transitions are permitted.");
        }

        var account = await _context.BankAccounts
            .FirstOrDefaultAsync(ba => ba.Id == accountId, cancellationToken);

        if (account == null)
        {
            throw new NotFoundException("Account", accountId);
        }

        if (account.Status == AccountStatus.Closed)
        {
            throw new BadRequestException("Cannot modify status of a CLOSED account.");
        }

        if (account.Status == newStatus)
        {
            return new AccountSummaryDto
            {
                Id = account.Id,
                AccountNumber = account.AccountNumber,
                AccountName = account.AccountName,
                AccountType = account.AccountType.ToString().ToUpperInvariant(),
                Balance = account.Balance,
                Currency = account.Currency,
                Status = account.Status.ToString().ToUpperInvariant()
            };
        }

        var oldStatus = account.Status;
        account.Status = newStatus;
        account.UpdatedAtUtc = DateTime.UtcNow;

        var action = newStatus == AccountStatus.Locked ? "ACCOUNT_LOCK" : "ACCOUNT_UNLOCK";
        var audit = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = adminUserId,
            Action = action,
            EntityType = "BankAccount",
            EntityId = account.Id.ToString(),
            Description = $"Admin changed account status from {oldStatus} to {newStatus}",
            IpAddress = ipAddress,
            CreatedAtUtc = DateTime.UtcNow
        };
        _context.AuditLogs.Add(audit);

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Bank Account {AccountId} status changed from {OldStatus} to {NewStatus} by Admin {AdminId}",
            accountId, oldStatus, newStatus, adminUserId);

        return new AccountSummaryDto
        {
            Id = account.Id,
            AccountNumber = account.AccountNumber,
            AccountName = account.AccountName,
            AccountType = account.AccountType.ToString().ToUpperInvariant(),
            Balance = account.Balance,
            Currency = account.Currency,
            Status = account.Status.ToString().ToUpperInvariant()
        };
    }
}
