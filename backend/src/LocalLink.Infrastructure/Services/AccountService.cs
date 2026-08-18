using LocalLink.Application.Accounts.DTOs;
using LocalLink.Application.Accounts.Interfaces;
using LocalLink.Application.Common.Exceptions;
using LocalLink.Domain.Enums;
using LocalLink.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LocalLink.Infrastructure.Services;

public class AccountService : IAccountService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AccountService> _logger;

    public AccountService(ApplicationDbContext context, ILogger<AccountService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<AccountSummaryDto>> GetMyAccountsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (customer == null)
        {
            throw new NotFoundException("Customer profile not found for the authenticated user.");
        }

        var accounts = await _context.BankAccounts
            .AsNoTracking()
            .Where(ba => ba.CustomerId == customer.Id)
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

    public async Task<AccountDetailDto> GetMyAccountDetailAsync(Guid userId, Guid accountId, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (customer == null)
        {
            throw new NotFoundException("Customer profile not found for the authenticated user.");
        }

        var account = await _context.BankAccounts
            .AsNoTracking()
            .FirstOrDefaultAsync(ba => ba.Id == accountId, cancellationToken);

        // Ownership enforcement: If account doesn't exist OR belongs to another customer, return 404 to avoid data leaks
        if (account == null || account.CustomerId != customer.Id)
        {
            throw new NotFoundException("Account", accountId);
        }

        return new AccountDetailDto
        {
            Id = account.Id,
            CustomerId = account.CustomerId,
            AccountNumber = account.AccountNumber,
            AccountName = account.AccountName,
            AccountType = account.AccountType.ToString().ToUpperInvariant(),
            Balance = account.Balance,
            Currency = account.Currency,
            Status = account.Status.ToString().ToUpperInvariant(),
            CreatedAtUtc = account.CreatedAtUtc
        };
    }

    public async Task<AccountLookupDto> LookupAccountAsync(string accountNumber, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
        {
            throw new BadRequestException("Account number is required.");
        }

        var cleanNumber = accountNumber.Trim();
        var account = await _context.BankAccounts
            .AsNoTracking()
            .FirstOrDefaultAsync(ba => ba.AccountNumber == cleanNumber && ba.Status != AccountStatus.Closed, cancellationToken);

        if (account == null)
        {
            throw new NotFoundException($"Active bank account with number '{cleanNumber}' was not found.");
        }

        // Return strictly minimal public information
        return new AccountLookupDto
        {
            AccountNumber = account.AccountNumber,
            AccountName = account.AccountName
        };
    }
}
