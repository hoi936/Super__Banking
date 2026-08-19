using System.Security.Cryptography;
using LocalLink.Application.Common.Exceptions;
using LocalLink.Application.Common.Models;
using LocalLink.Application.Napas.DTOs;
using LocalLink.Application.Napas.Interfaces;
using LocalLink.Domain.Entities;
using LocalLink.Domain.Enums;
using LocalLink.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LocalLink.Infrastructure.Services;

public class NapasTransferService : INapasTransferService
{
    private const decimal FlatFee = 3300m;
    private static readonly IReadOnlyList<NapasBankDto> Banks =
    [
        new() { Code = "VCB", Name = "Ngan hang TMCP Ngoai thuong Viet Nam", ShortName = "Vietcombank" },
        new() { Code = "BIDV", Name = "Ngan hang TMCP Dau tu va Phat trien Viet Nam", ShortName = "BIDV" },
        new() { Code = "TCB", Name = "Ngan hang TMCP Ky Thuong Viet Nam", ShortName = "Techcombank" },
        new() { Code = "ACB", Name = "Ngan hang TMCP A Chau", ShortName = "ACB" },
        new() { Code = "MB", Name = "Ngan hang TMCP Quan doi", ShortName = "MBBank" },
        new() { Code = "VPB", Name = "Ngan hang TMCP Viet Nam Thinh Vuong", ShortName = "VPBank" },
        new() { Code = "MOMO", Name = "Momo Digital Payment Demo Bank", ShortName = "Momo Demo", SupportsCardTransfer = false }
    ];

    private static readonly string[] DemoFamilyNames = ["Nguyen", "Tran", "Le", "Pham", "Hoang", "Phan", "Vu", "Dang"];
    private static readonly string[] DemoMiddleNames = ["Van", "Thi", "Minh", "Thanh", "Duc", "Quang", "Gia", "Bao"];
    private static readonly string[] DemoGivenNames = ["An", "Binh", "Chi", "Dung", "Huy", "Linh", "Nam", "Trang"];

    private readonly ApplicationDbContext _context;
    private readonly ILogger<NapasTransferService> _logger;

    public NapasTransferService(ApplicationDbContext context, ILogger<NapasTransferService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public Task<IReadOnlyList<NapasBankDto>> GetBanksAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Banks);
    }

    public Task<NapasLookupResultDto> LookupAsync(NapasLookupRequest request, CancellationToken cancellationToken = default)
    {
        var bank = ResolveBank(request.BankCode);
        var destinationType = NormalizeDestinationType(request.DestinationType);
        var destinationNumber = NormalizeDestinationNumber(request.DestinationNumber);

        EnsureDestinationSupported(bank, destinationType);

        return Task.FromResult(new NapasLookupResultDto
        {
            BankCode = bank.Code,
            BankName = bank.ShortName,
            DestinationNumber = destinationNumber,
            DestinationName = BuildDemoReceiverName(bank.Code, destinationNumber),
            DestinationType = destinationType
        });
    }

    public async Task<NapasTransferReceiptDto> TransferAsync(
        CreateNapasTransferRequest request,
        string? idempotencyKey,
        Guid currentUserId,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        if (request.Amount <= 0)
        {
            throw new BadRequestException("Transfer amount must be greater than zero.");
        }

        var bank = ResolveBank(request.BankCode);
        var destinationType = NormalizeDestinationType(request.DestinationType);
        var destinationNumber = NormalizeDestinationNumber(request.DestinationNumber);
        EnsureDestinationSupported(bank, destinationType);

        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == currentUserId, cancellationToken);

        if (customer == null)
        {
            throw new NotFoundException("Customer", currentUserId);
        }

        var cleanKey = string.IsNullOrWhiteSpace(idempotencyKey) ? null : idempotencyKey.Trim();
        if (cleanKey != null)
        {
            var existing = await _context.ExternalTransfers
                .AsNoTracking()
                .Include(et => et.Transaction)
                .Include(et => et.SourceAccount)
                .FirstOrDefaultAsync(et => et.IdempotencyKey == cleanKey, cancellationToken);

            if (existing != null)
            {
                if (existing.SourceAccount.CustomerId != customer.Id)
                {
                    throw new ForbiddenException("Access denied to the specified idempotency key.");
                }

                var isSamePayload = existing.SourceAccountId == request.SourceAccountId
                    && string.Equals(existing.ExternalBankCode, bank.Code, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(existing.DestinationAccountNumber, destinationNumber, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(existing.DestinationType, destinationType, StringComparison.OrdinalIgnoreCase)
                    && existing.Amount == request.Amount;

                if (!isSamePayload)
                {
                    throw new ConflictException("Idempotency key was previously used with a different Napas payload.");
                }

                return BuildReceipt(existing, existing.SourceAccount.Balance);
            }
        }

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

        if (!string.Equals(sourceAccount.Currency, "VND", StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestException("Napas 24/7 simulation currently supports VND accounts only.");
        }

        var feeAmount = CalculateFee(request.Amount);
        var totalDebit = request.Amount + feeAmount;
        if (sourceAccount.Balance < totalDebit)
        {
            throw new BadRequestException("Insufficient funds for transfer amount and Napas fee.");
        }

        var destinationName = BuildDemoReceiverName(bank.Code, destinationNumber);
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var dbTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                sourceAccount.Balance -= totalDebit;
                sourceAccount.UpdatedAtUtc = DateTime.UtcNow;

                var referenceNumber = $"NPS{DateTime.UtcNow:yyyyMMddHHmmss}{RandomNumberGenerator.GetInt32(1000, 9999)}";
                var now = DateTime.UtcNow;

                var transactionEntity = new Transaction
                {
                    Id = Guid.NewGuid(),
                    ReferenceNumber = referenceNumber,
                    TransactionType = TransactionType.NapasTransfer,
                    SourceAccountId = sourceAccount.Id,
                    DestinationAccountId = null,
                    Amount = totalDebit,
                    Currency = sourceAccount.Currency,
                    Description = request.Description ?? $"Napas transfer to {bank.ShortName}",
                    Status = TransactionStatus.Completed,
                    CreatedAtUtc = now,
                    CompletedAtUtc = now
                };
                _context.Transactions.Add(transactionEntity);

                var externalTransfer = new ExternalTransfer
                {
                    Id = Guid.NewGuid(),
                    TransactionId = transactionEntity.Id,
                    SourceAccountId = sourceAccount.Id,
                    ExternalBankCode = bank.Code,
                    ExternalBankName = bank.ShortName,
                    DestinationAccountNumber = destinationNumber,
                    DestinationAccountName = destinationName,
                    DestinationType = destinationType,
                    Amount = request.Amount,
                    FeeAmount = feeAmount,
                    Currency = sourceAccount.Currency,
                    Description = request.Description,
                    IdempotencyKey = cleanKey,
                    Status = ExternalTransferStatus.Completed,
                    CreatedAtUtc = now,
                    CompletedAtUtc = now
                };
                _context.ExternalTransfers.Add(externalTransfer);

                _context.AuditLogs.Add(new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = currentUserId,
                    Action = "NAPAS_TRANSFER_COMPLETED",
                    EntityType = "ExternalTransfer",
                    EntityId = externalTransfer.Id.ToString(),
                    Description = $"Napas transfer {request.Amount:N2} {sourceAccount.Currency} from {sourceAccount.AccountNumber} to {bank.ShortName} {destinationNumber}. Fee: {feeAmount:N2}. Reference: {referenceNumber}",
                    IpAddress = ipAddress,
                    CreatedAtUtc = now
                });

                _context.Notifications.Add(new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = currentUserId,
                    Title = "Chuyen tien Napas thanh cong",
                    Message = $"Ban da chuyen {request.Amount:N0} {sourceAccount.Currency} den {destinationName} tai {bank.ShortName}. Phi: {feeAmount:N0} {sourceAccount.Currency}.",
                    Type = NotificationType.Transfer,
                    IsRead = false,
                    CreatedAtUtc = now
                });

                await _context.SaveChangesAsync(cancellationToken);
                await dbTransaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Napas transfer {Reference} completed: {Amount} {Currency} to {Bank}/{Destination}",
                    referenceNumber, request.Amount, sourceAccount.Currency, bank.Code, destinationNumber);

                return BuildReceipt(externalTransfer, sourceAccount.Balance, referenceNumber);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await dbTransaction.RollbackAsync(cancellationToken);
                _logger.LogWarning(ex, "Concurrency conflict during Napas transfer for Account {AccountId}", sourceAccount.Id);
                throw new ConflictException("A concurrency conflict occurred while processing the Napas transfer. Please refresh your balance and try again.");
            }
            catch
            {
                await dbTransaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    public async Task<PagedResult<NapasTransferListItemDto>> GetMyTransfersAsync(
        Guid currentUserId,
        int page = 1,
        int pageSize = 20,
        string? status = null,
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

        var query = _context.ExternalTransfers
            .AsNoTracking()
            .Include(et => et.Transaction)
            .Include(et => et.SourceAccount)
            .Where(et => et.SourceAccount.CustomerId == customer.Id);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ExternalTransferStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(et => et.Status == parsedStatus);
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(et => et.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(et => new NapasTransferListItemDto
            {
                Id = et.Id,
                Reference = et.Transaction.ReferenceNumber,
                SourceAccountNumber = et.SourceAccount.AccountNumber,
                ExternalBankCode = et.ExternalBankCode,
                ExternalBankName = et.ExternalBankName,
                DestinationNumber = et.DestinationAccountNumber,
                DestinationName = et.DestinationAccountName,
                DestinationType = et.DestinationType,
                Amount = et.Amount,
                FeeAmount = et.FeeAmount,
                Currency = et.Currency,
                Status = et.Status.ToString().ToUpperInvariant(),
                CreatedAtUtc = et.CreatedAtUtc,
                CompletedAtUtc = et.CompletedAtUtc
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<NapasTransferListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    private static NapasBankDto ResolveBank(string bankCode)
    {
        if (string.IsNullOrWhiteSpace(bankCode))
        {
            throw new BadRequestException("Bank code is required.");
        }

        var cleanCode = bankCode.Trim().ToUpperInvariant();
        return Banks.FirstOrDefault(bank => bank.Code == cleanCode)
            ?? throw new NotFoundException($"Napas bank '{cleanCode}' was not found.");
    }

    private static string NormalizeDestinationType(string destinationType)
    {
        var cleanType = string.IsNullOrWhiteSpace(destinationType)
            ? "ACCOUNT"
            : destinationType.Trim().ToUpperInvariant();

        if (cleanType is not ("ACCOUNT" or "CARD"))
        {
            throw new BadRequestException("Destination type must be ACCOUNT or CARD.");
        }

        return cleanType;
    }

    private static string NormalizeDestinationNumber(string destinationNumber)
    {
        if (string.IsNullOrWhiteSpace(destinationNumber))
        {
            throw new BadRequestException("Destination account or card number is required.");
        }

        var cleanNumber = new string(destinationNumber.Where(char.IsDigit).ToArray());
        if (cleanNumber.Length < 6 || cleanNumber.Length > 19)
        {
            throw new BadRequestException("Destination number must contain 6 to 19 digits.");
        }

        return cleanNumber;
    }

    private static void EnsureDestinationSupported(NapasBankDto bank, string destinationType)
    {
        if (destinationType == "CARD" && !bank.SupportsCardTransfer)
        {
            throw new BadRequestException($"{bank.ShortName} does not support card transfers in this simulation.");
        }

        if (destinationType == "ACCOUNT" && !bank.SupportsAccountTransfer)
        {
            throw new BadRequestException($"{bank.ShortName} does not support account transfers in this simulation.");
        }
    }

    private static decimal CalculateFee(decimal amount)
    {
        if (amount >= 100000000m)
        {
            return 11000m;
        }

        if (amount >= 10000000m)
        {
            return 5500m;
        }

        return FlatFee;
    }

    private static string BuildDemoReceiverName(string bankCode, string destinationNumber)
    {
        var seed = Math.Abs(HashCode.Combine(bankCode, destinationNumber));
        var family = DemoFamilyNames[seed % DemoFamilyNames.Length];
        var middle = DemoMiddleNames[(seed / 7) % DemoMiddleNames.Length];
        var given = DemoGivenNames[(seed / 13) % DemoGivenNames.Length];
        return $"{family} {middle} {given}".ToUpperInvariant();
    }

    private static NapasTransferReceiptDto BuildReceipt(ExternalTransfer transfer, decimal remainingBalance, string? reference = null)
    {
        return new NapasTransferReceiptDto
        {
            ExternalTransferId = transfer.Id,
            Reference = reference ?? transfer.Transaction?.ReferenceNumber ?? string.Empty,
            SourceAccountId = transfer.SourceAccountId,
            SourceAccountNumber = transfer.SourceAccount.AccountNumber,
            ExternalBankCode = transfer.ExternalBankCode,
            ExternalBankName = transfer.ExternalBankName,
            DestinationNumber = transfer.DestinationAccountNumber,
            DestinationName = transfer.DestinationAccountName,
            DestinationType = transfer.DestinationType,
            Amount = transfer.Amount,
            FeeAmount = transfer.FeeAmount,
            TotalDebitAmount = transfer.Amount + transfer.FeeAmount,
            RemainingBalance = remainingBalance,
            Currency = transfer.Currency,
            Description = transfer.Description,
            Status = transfer.Status.ToString().ToUpperInvariant(),
            CreatedAtUtc = transfer.CreatedAtUtc,
            CompletedAtUtc = transfer.CompletedAtUtc
        };
    }
}
