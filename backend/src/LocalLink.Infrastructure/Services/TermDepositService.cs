using System.Security.Cryptography;
using LocalLink.Application.Common.Exceptions;
using LocalLink.Application.TermDeposits.DTOs;
using LocalLink.Application.TermDeposits.Interfaces;
using LocalLink.Domain.Entities;
using LocalLink.Domain.Enums;
using LocalLink.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LocalLink.Infrastructure.Services;

public class TermDepositService : ITermDepositService
{
    private static readonly IReadOnlyDictionary<int, decimal> AnnualRates = new Dictionary<int, decimal>
    {
        [1] = 4.20m,
        [3] = 4.80m,
        [12] = 5.80m
    };

    private readonly ApplicationDbContext _context;
    private readonly ILogger<TermDepositService> _logger;

    public TermDepositService(ApplicationDbContext context, ILogger<TermDepositService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TermDepositDto>> GetMyTermDepositsAsync(
        Guid currentUserId,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var customer = await GetCustomerAsync(currentUserId, true, cancellationToken);

        var query = _context.TermDeposits
            .AsNoTracking()
            .Include(td => td.SourceAccount)
            .Where(td => td.CustomerId == customer.Id);

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<TermDepositStatus>(status, true, out var parsedStatus))
            {
                throw new BadRequestException("Invalid term deposit status.");
            }

            query = query.Where(td => td.Status == parsedStatus);
        }

        var termDeposits = await query
            .OrderByDescending(td => td.OpenedAtUtc)
            .ToListAsync(cancellationToken);

        return termDeposits.Select(MapToDto).ToList();
    }

    public async Task<TermDepositDetailDto> GetMyTermDepositDetailAsync(
        Guid currentUserId,
        Guid termDepositId,
        CancellationToken cancellationToken = default)
    {
        var customer = await GetCustomerAsync(currentUserId, true, cancellationToken);

        var termDeposit = await _context.TermDeposits
            .AsNoTracking()
            .Include(td => td.SourceAccount)
            .FirstOrDefaultAsync(td => td.Id == termDepositId, cancellationToken);

        if (termDeposit == null || termDeposit.CustomerId != customer.Id)
        {
            throw new NotFoundException("Term deposit", termDepositId);
        }

        return MapToDetailDto(termDeposit);
    }

    public async Task<TermDepositReceiptDto> OpenTermDepositAsync(
        CreateTermDepositRequest request,
        string? idempotencyKey,
        Guid currentUserId,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        var customer = await GetCustomerAsync(currentUserId, false, cancellationToken);
        var cleanKey = string.IsNullOrWhiteSpace(idempotencyKey) ? null : idempotencyKey.Trim();
        var annualRate = ResolveRate(request.TenorMonths);

        if (cleanKey != null)
        {
            var existingTermDeposit = await _context.TermDeposits
                .Include(td => td.SourceAccount)
                .Include(td => td.OpeningTransaction)
                .FirstOrDefaultAsync(td => td.IdempotencyKey == cleanKey, cancellationToken);

            if (existingTermDeposit != null)
            {
                if (existingTermDeposit.CustomerId == customer.Id
                    && existingTermDeposit.SourceAccountId == request.SourceAccountId
                    && existingTermDeposit.PrincipalAmount == request.PrincipalAmount
                    && existingTermDeposit.TenorMonths == request.TenorMonths)
                {
                    return MapToReceiptDto(existingTermDeposit);
                }

                throw new ConflictException("Idempotency key was previously used with a different term deposit payload.");
            }
        }

        var account = await _context.BankAccounts
            .FirstOrDefaultAsync(a => a.Id == request.SourceAccountId, cancellationToken);

        if (account == null || account.CustomerId != customer.Id)
        {
            throw new NotFoundException("Source bank account not found.");
        }

        if (account.Status != AccountStatus.Active)
        {
            throw new BadRequestException($"Source bank account is {account.Status.ToString().ToUpperInvariant()} and cannot open term deposits.");
        }

        if (account.Currency != "VND")
        {
            throw new BadRequestException("Only VND term deposits are supported in V2.");
        }

        if (request.PrincipalAmount <= 0)
        {
            throw new BadRequestException("Principal amount must be greater than zero.");
        }

        if (account.Balance < request.PrincipalAmount)
        {
            throw new BadRequestException("Insufficient funds in source account to open term deposit.");
        }

        var openedAtUtc = DateTime.UtcNow;
        var maturityDateUtc = openedAtUtc.AddMonths(request.TenorMonths);
        var expectedInterest = CalculateInterest(request.PrincipalAmount, annualRate, request.TenorMonths);

        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var dbTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            account.Balance -= request.PrincipalAmount;
            account.UpdatedAtUtc = DateTime.UtcNow;

            var referenceNumber = GenerateReference("TD");
            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                ReferenceNumber = referenceNumber,
                TransactionType = TransactionType.TermDepositOpening,
                SourceAccountId = account.Id,
                Amount = request.PrincipalAmount,
                Currency = account.Currency,
                Description = $"Open term deposit {request.TenorMonths}M at {annualRate:N2}% p.a.",
                Status = TransactionStatus.Completed,
                CreatedAtUtc = DateTime.UtcNow,
                CompletedAtUtc = DateTime.UtcNow
            };
            _context.Transactions.Add(transaction);

            var termDeposit = new TermDeposit
            {
                Id = Guid.NewGuid(),
                CustomerId = customer.Id,
                SourceAccountId = account.Id,
                OpeningTransactionId = transaction.Id,
                DepositNumber = GenerateReference("TDNO"),
                PrincipalAmount = request.PrincipalAmount,
                AnnualInterestRate = annualRate,
                TenorMonths = request.TenorMonths,
                ExpectedInterestAmount = expectedInterest,
                Currency = account.Currency,
                Status = TermDepositStatus.Active,
                IdempotencyKey = cleanKey,
                OpenedAtUtc = openedAtUtc,
                MaturityDateUtc = maturityDateUtc,
                CreatedAtUtc = DateTime.UtcNow
            };
            _context.TermDeposits.Add(termDeposit);

            _context.AuditLogs.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = currentUserId,
                Action = "TERM_DEPOSIT_OPENED",
                EntityType = "TermDeposit",
                EntityId = termDeposit.Id.ToString(),
                Description = $"Opened term deposit {termDeposit.DepositNumber} for {request.PrincipalAmount:N2} {account.Currency}, tenor {request.TenorMonths} months.",
                IpAddress = ipAddress,
                CreatedAtUtc = DateTime.UtcNow
            });

            _context.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = currentUserId,
                Title = "Mở sổ tiết kiệm thành công",
                Message = $"Sổ tiết kiệm {termDeposit.DepositNumber} kỳ hạn {request.TenorMonths} tháng đã được mở thành công.",
                Type = NotificationType.Account,
                IsRead = false,
                CreatedAtUtc = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);
            await dbTransaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Term deposit {DepositNumber} opened for customer {CustomerId}", termDeposit.DepositNumber, customer.Id);

            termDeposit.SourceAccount = account;
            termDeposit.OpeningTransaction = transaction;
            return MapToReceiptDto(termDeposit);
        });
    }

    public async Task<int> MatureDueTermDepositsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var dueTermDeposits = await _context.TermDeposits
            .Include(td => td.Customer)
            .Include(td => td.SourceAccount)
            .Where(td => td.Status == TermDepositStatus.Active && td.MaturityDateUtc <= now)
            .OrderBy(td => td.MaturityDateUtc)
            .ToListAsync(cancellationToken);

        var maturedCount = 0;
        var strategy = _context.Database.CreateExecutionStrategy();

        foreach (var termDeposit in dueTermDeposits)
        {
            await strategy.ExecuteAsync(async () =>
            {
                await using var dbTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);

                var payoutAmount = termDeposit.PrincipalAmount + termDeposit.ExpectedInterestAmount;
                termDeposit.SourceAccount.Balance += payoutAmount;
                termDeposit.SourceAccount.UpdatedAtUtc = DateTime.UtcNow;

                var transaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    ReferenceNumber = GenerateReference("TDM"),
                    TransactionType = TransactionType.TermDepositMaturity,
                    DestinationAccountId = termDeposit.SourceAccountId,
                    Amount = payoutAmount,
                    Currency = termDeposit.Currency,
                    Description = $"Mature term deposit {termDeposit.DepositNumber}",
                    Status = TransactionStatus.Completed,
                    CreatedAtUtc = DateTime.UtcNow,
                    CompletedAtUtc = DateTime.UtcNow
                };
                _context.Transactions.Add(transaction);

                termDeposit.MaturityTransactionId = transaction.Id;
                termDeposit.Status = TermDepositStatus.Matured;
                termDeposit.PaidInterestAmount = termDeposit.ExpectedInterestAmount;
                termDeposit.ClosedAtUtc = DateTime.UtcNow;
                termDeposit.UpdatedAtUtc = DateTime.UtcNow;

                _context.AuditLogs.Add(new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = null,
                    Action = "TERM_DEPOSIT_MATURED",
                    EntityType = "TermDeposit",
                    EntityId = termDeposit.Id.ToString(),
                    Description = $"Term deposit {termDeposit.DepositNumber} matured with payout {payoutAmount:N2} {termDeposit.Currency}.",
                    CreatedAtUtc = DateTime.UtcNow
                });

                _context.Notifications.Add(new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = termDeposit.Customer.UserId,
                    Title = "Sổ tiết kiệm đã đến hạn",
                    Message = $"Sổ tiết kiệm {termDeposit.DepositNumber} đã tất toán. Gốc và lãi đã được cộng vào tài khoản.",
                    Type = NotificationType.Account,
                    IsRead = false,
                    CreatedAtUtc = DateTime.UtcNow
                });

                await _context.SaveChangesAsync(cancellationToken);
                await dbTransaction.CommitAsync(cancellationToken);
                maturedCount++;
            });
        }

        return maturedCount;
    }

    private async Task<Customer> GetCustomerAsync(Guid currentUserId, bool asNoTracking, CancellationToken cancellationToken)
    {
        var query = _context.Customers.AsQueryable();

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        var customer = await query.FirstOrDefaultAsync(c => c.UserId == currentUserId, cancellationToken);
        return customer ?? throw new NotFoundException("Customer profile not found.");
    }

    private static decimal ResolveRate(int tenorMonths)
    {
        if (!AnnualRates.TryGetValue(tenorMonths, out var rate))
        {
            throw new BadRequestException("Unsupported tenor. Allowed tenors are 1, 3, and 12 months.");
        }

        return rate;
    }

    private static decimal CalculateInterest(decimal principal, decimal annualRate, int tenorMonths)
    {
        return Math.Round(principal * annualRate / 100m * tenorMonths / 12m, 2, MidpointRounding.AwayFromZero);
    }

    private static string GenerateReference(string prefix)
    {
        return $"{prefix}{DateTime.UtcNow:yyyyMMddHHmmss}{RandomNumberGenerator.GetInt32(1000, 9999)}";
    }

    private static TermDepositDto MapToDto(TermDeposit termDeposit)
    {
        return new TermDepositDto
        {
            Id = termDeposit.Id,
            DepositNumber = termDeposit.DepositNumber,
            SourceAccountId = termDeposit.SourceAccountId,
            SourceAccountNumber = termDeposit.SourceAccount?.AccountNumber ?? string.Empty,
            PrincipalAmount = termDeposit.PrincipalAmount,
            AnnualInterestRate = termDeposit.AnnualInterestRate,
            TenorMonths = termDeposit.TenorMonths,
            ExpectedInterestAmount = termDeposit.ExpectedInterestAmount,
            ExpectedPayoutAmount = termDeposit.PrincipalAmount + termDeposit.ExpectedInterestAmount,
            Currency = termDeposit.Currency,
            Status = termDeposit.Status.ToString().ToUpperInvariant(),
            OpenedAtUtc = termDeposit.OpenedAtUtc,
            MaturityDateUtc = termDeposit.MaturityDateUtc,
            ClosedAtUtc = termDeposit.ClosedAtUtc
        };
    }

    private static TermDepositDetailDto MapToDetailDto(TermDeposit termDeposit)
    {
        var dto = new TermDepositDetailDto
        {
            OpeningTransactionId = termDeposit.OpeningTransactionId,
            MaturityTransactionId = termDeposit.MaturityTransactionId,
            PaidInterestAmount = termDeposit.PaidInterestAmount,
            IdempotencyKey = termDeposit.IdempotencyKey,
            CreatedAtUtc = termDeposit.CreatedAtUtc,
            UpdatedAtUtc = termDeposit.UpdatedAtUtc
        };

        var summary = MapToDto(termDeposit);
        dto.Id = summary.Id;
        dto.DepositNumber = summary.DepositNumber;
        dto.SourceAccountId = summary.SourceAccountId;
        dto.SourceAccountNumber = summary.SourceAccountNumber;
        dto.PrincipalAmount = summary.PrincipalAmount;
        dto.AnnualInterestRate = summary.AnnualInterestRate;
        dto.TenorMonths = summary.TenorMonths;
        dto.ExpectedInterestAmount = summary.ExpectedInterestAmount;
        dto.ExpectedPayoutAmount = summary.ExpectedPayoutAmount;
        dto.Currency = summary.Currency;
        dto.Status = summary.Status;
        dto.OpenedAtUtc = summary.OpenedAtUtc;
        dto.MaturityDateUtc = summary.MaturityDateUtc;
        dto.ClosedAtUtc = summary.ClosedAtUtc;

        return dto;
    }

    private static TermDepositReceiptDto MapToReceiptDto(TermDeposit termDeposit)
    {
        return new TermDepositReceiptDto
        {
            TermDepositId = termDeposit.Id,
            DepositNumber = termDeposit.DepositNumber,
            Reference = termDeposit.OpeningTransaction?.ReferenceNumber ?? string.Empty,
            SourceAccountNumber = termDeposit.SourceAccount?.AccountNumber ?? string.Empty,
            PrincipalAmount = termDeposit.PrincipalAmount,
            AnnualInterestRate = termDeposit.AnnualInterestRate,
            TenorMonths = termDeposit.TenorMonths,
            ExpectedInterestAmount = termDeposit.ExpectedInterestAmount,
            ExpectedPayoutAmount = termDeposit.PrincipalAmount + termDeposit.ExpectedInterestAmount,
            Currency = termDeposit.Currency,
            Status = termDeposit.Status.ToString().ToUpperInvariant(),
            OpenedAtUtc = termDeposit.OpenedAtUtc,
            MaturityDateUtc = termDeposit.MaturityDateUtc
        };
    }
}
