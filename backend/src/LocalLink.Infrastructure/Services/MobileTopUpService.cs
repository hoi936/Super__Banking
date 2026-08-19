using System.Security.Cryptography;
using System.Text.RegularExpressions;
using LocalLink.Application.Common.Exceptions;
using LocalLink.Application.Common.Models;
using LocalLink.Application.MobileTopUps.DTOs;
using LocalLink.Application.MobileTopUps.Interfaces;
using LocalLink.Domain.Entities;
using LocalLink.Domain.Enums;
using LocalLink.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LocalLink.Infrastructure.Services;

public partial class MobileTopUpService : IMobileTopUpService
{
    private static readonly IReadOnlyList<MobileProviderDto> Providers =
    [
        BuildProvider("VIETTEL", "Viettel"),
        BuildProvider("VINAPHONE", "VinaPhone"),
        BuildProvider("MOBIFONE", "MobiFone"),
        BuildProvider("VIETNAMOBILE", "Vietnamobile")
    ];

    private readonly ApplicationDbContext _context;
    private readonly ILogger<MobileTopUpService> _logger;

    public MobileTopUpService(ApplicationDbContext context, ILogger<MobileTopUpService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public Task<IReadOnlyList<MobileProviderDto>> GetProvidersAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Providers);
    }

    public async Task<MobileTopUpReceiptDto> PurchaseAsync(
        CreateMobileTopUpRequest request,
        string? idempotencyKey,
        Guid currentUserId,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        var provider = ResolveProvider(request.ProviderCode);
        var product = ResolveProduct(provider, request.ProductCode);
        var productType = Enum.Parse<MobileTopUpProductType>(product.ProductType, true);
        var phoneNumber = NormalizePhoneNumber(request.PhoneNumber);

        if (productType != MobileTopUpProductType.CardCode && string.IsNullOrWhiteSpace(phoneNumber))
        {
            throw new BadRequestException("Phone number is required for top-up and data package purchases.");
        }

        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(customer => customer.UserId == currentUserId, cancellationToken);

        if (customer == null)
        {
            throw new NotFoundException("Customer", currentUserId);
        }

        var cleanKey = string.IsNullOrWhiteSpace(idempotencyKey) ? null : idempotencyKey.Trim();
        if (cleanKey != null)
        {
            var existing = await _context.MobileTopUps
                .AsNoTracking()
                .Include(topUp => topUp.SourceAccount)
                .Include(topUp => topUp.Transaction)
                .FirstOrDefaultAsync(topUp => topUp.IdempotencyKey == cleanKey, cancellationToken);

            if (existing != null)
            {
                if (existing.CustomerId != customer.Id)
                {
                    throw new ForbiddenException("Access denied to the specified idempotency key.");
                }

                var samePayload = existing.SourceAccountId == request.SourceAccountId
                    && string.Equals(existing.ProviderCode, provider.Code, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(existing.ProductCode, product.Code, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(existing.PhoneNumber, phoneNumber, StringComparison.OrdinalIgnoreCase);

                if (!samePayload)
                {
                    throw new ConflictException("Idempotency key was previously used with a different mobile top-up payload.");
                }

                return BuildReceipt(existing, existing.SourceAccount.Balance);
            }
        }

        var sourceAccount = await _context.BankAccounts
            .FirstOrDefaultAsync(account => account.Id == request.SourceAccountId, cancellationToken);

        if (sourceAccount == null || sourceAccount.CustomerId != customer.Id)
        {
            throw new NotFoundException("Source bank account not found or not owned by the current customer.");
        }

        if (sourceAccount.Status != AccountStatus.Active)
        {
            throw new BadRequestException($"Source account is {sourceAccount.Status} and cannot perform mobile top-ups.");
        }

        if (!string.Equals(sourceAccount.Currency, "VND", StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestException("Mobile top-up currently supports VND accounts only.");
        }

        if (sourceAccount.Balance < product.Amount)
        {
            throw new BadRequestException("Insufficient funds for mobile top-up.");
        }

        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var dbTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var now = DateTime.UtcNow;
                sourceAccount.Balance -= product.Amount;
                sourceAccount.UpdatedAtUtc = now;

                var referenceNumber = GenerateReference("MTP");
                var transaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    ReferenceNumber = referenceNumber,
                    TransactionType = TransactionType.MobileTopUp,
                    SourceAccountId = sourceAccount.Id,
                    Amount = product.Amount,
                    Currency = sourceAccount.Currency,
                    Description = BuildDescription(provider, product, phoneNumber),
                    Status = TransactionStatus.Completed,
                    CreatedAtUtc = now,
                    CompletedAtUtc = now
                };
                _context.Transactions.Add(transaction);

                var topUp = new MobileTopUp
                {
                    Id = Guid.NewGuid(),
                    CustomerId = customer.Id,
                    SourceAccountId = sourceAccount.Id,
                    TransactionId = transaction.Id,
                    ProviderCode = provider.Code,
                    ProviderName = provider.Name,
                    ProductType = productType,
                    ProductCode = product.Code,
                    ProductName = product.Name,
                    PhoneNumber = productType == MobileTopUpProductType.CardCode ? null : phoneNumber,
                    Amount = product.Amount,
                    Currency = sourceAccount.Currency,
                    CardSerial = productType == MobileTopUpProductType.CardCode ? GenerateDigits(14) : null,
                    CardPin = productType == MobileTopUpProductType.CardCode ? GenerateDigits(12) : null,
                    Status = MobileTopUpStatus.Completed,
                    IdempotencyKey = cleanKey,
                    CreatedAtUtc = now,
                    CompletedAtUtc = now
                };
                _context.MobileTopUps.Add(topUp);

                _context.AuditLogs.Add(new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = currentUserId,
                    Action = "MOBILE_TOPUP_COMPLETED",
                    EntityType = "MobileTopUp",
                    EntityId = topUp.Id.ToString(),
                    Description = $"{product.Name} {provider.Name} for {product.Amount:N2} {sourceAccount.Currency}. Reference: {referenceNumber}",
                    IpAddress = ipAddress,
                    CreatedAtUtc = now
                });

                _context.Notifications.Add(new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = currentUserId,
                    Title = "Dich vu di dong thanh cong",
                    Message = productType == MobileTopUpProductType.CardCode
                        ? $"Ban da mua ma the {provider.Name} menh gia {product.Amount:N0} {sourceAccount.Currency}. Ma giao dich: {referenceNumber}."
                        : $"Ban da nap {product.Name} cho so {phoneNumber} thanh cong. Ma giao dich: {referenceNumber}.",
                    Type = NotificationType.Payment,
                    IsRead = false,
                    CreatedAtUtc = now
                });

                await _context.SaveChangesAsync(cancellationToken);
                await dbTransaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Mobile top-up {Reference} completed for customer {CustomerId}", referenceNumber, customer.Id);

                topUp.SourceAccount = sourceAccount;
                topUp.Transaction = transaction;
                return BuildReceipt(topUp, sourceAccount.Balance);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await dbTransaction.RollbackAsync(cancellationToken);
                _logger.LogWarning(ex, "Concurrency conflict during mobile top-up for account {AccountId}", sourceAccount.Id);
                throw new ConflictException("A concurrency conflict occurred while processing the mobile top-up. Please refresh your balance and try again.");
            }
            catch
            {
                await dbTransaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    public async Task<PagedResult<MobileTopUpListItemDto>> GetMyPurchasesAsync(
        Guid currentUserId,
        int page = 1,
        int pageSize = 20,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(customer => customer.UserId == currentUserId, cancellationToken);

        if (customer == null)
        {
            throw new NotFoundException("Customer", currentUserId);
        }

        var query = _context.MobileTopUps
            .AsNoTracking()
            .Include(topUp => topUp.SourceAccount)
            .Include(topUp => topUp.Transaction)
            .Where(topUp => topUp.CustomerId == customer.Id);

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<MobileTopUpStatus>(status, true, out var parsedStatus))
            {
                throw new BadRequestException("Invalid mobile top-up status.");
            }

            query = query.Where(topUp => topUp.Status == parsedStatus);
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(topUp => topUp.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<MobileTopUpListItemDto>
        {
            Items = items.Select(MapToListItem).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    private static MobileTopUpListItemDto MapToListItem(MobileTopUp topUp)
    {
        return new MobileTopUpListItemDto
        {
            Id = topUp.Id,
            Reference = topUp.Transaction.ReferenceNumber,
            SourceAccountNumber = topUp.SourceAccount.AccountNumber,
            ProviderName = topUp.ProviderName,
            ProductType = topUp.ProductType.ToString(),
            ProductName = topUp.ProductName,
            PhoneNumber = topUp.PhoneNumber,
            Amount = topUp.Amount,
            Currency = topUp.Currency,
            Status = topUp.Status.ToString(),
            CreatedAtUtc = topUp.CreatedAtUtc
        };
    }

    private static MobileTopUpReceiptDto BuildReceipt(MobileTopUp topUp, decimal remainingBalance)
    {
        return new MobileTopUpReceiptDto
        {
            MobileTopUpId = topUp.Id,
            Reference = topUp.Transaction.ReferenceNumber,
            SourceAccountNumber = topUp.SourceAccount.AccountNumber,
            ProviderCode = topUp.ProviderCode,
            ProviderName = topUp.ProviderName,
            ProductType = topUp.ProductType.ToString(),
            ProductName = topUp.ProductName,
            PhoneNumber = topUp.PhoneNumber,
            Amount = topUp.Amount,
            RemainingBalance = remainingBalance,
            Currency = topUp.Currency,
            CardSerial = topUp.CardSerial,
            CardPin = topUp.CardPin,
            Status = topUp.Status.ToString(),
            CreatedAtUtc = topUp.CreatedAtUtc,
            CompletedAtUtc = topUp.CompletedAtUtc
        };
    }

    private static MobileProviderDto ResolveProvider(string providerCode)
    {
        var provider = Providers.FirstOrDefault(item =>
            string.Equals(item.Code, providerCode?.Trim(), StringComparison.OrdinalIgnoreCase));

        return provider ?? throw new BadRequestException("Unsupported mobile provider.");
    }

    private static MobileProductDto ResolveProduct(MobileProviderDto provider, string productCode)
    {
        var product = provider.Products.FirstOrDefault(item =>
            string.Equals(item.Code, productCode?.Trim(), StringComparison.OrdinalIgnoreCase));

        return product ?? throw new BadRequestException("Unsupported mobile product.");
    }

    private static string? NormalizePhoneNumber(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var digits = DigitsOnlyRegex().Replace(value, string.Empty);
        if (digits.Length is < 9 or > 11)
        {
            throw new BadRequestException("Phone number must contain 9 to 11 digits.");
        }

        return digits;
    }

    private static string BuildDescription(MobileProviderDto provider, MobileProductDto product, string? phoneNumber)
    {
        return string.IsNullOrWhiteSpace(phoneNumber)
            ? $"Purchase {product.Name} from {provider.Name}"
            : $"Purchase {product.Name} from {provider.Name} for {phoneNumber}";
    }

    private static string GenerateReference(string prefix)
    {
        return $"{prefix}{DateTime.UtcNow:yyyyMMddHHmmss}{RandomNumberGenerator.GetInt32(1000, 9999)}";
    }

    private static string GenerateDigits(int length)
    {
        var chars = new char[length];
        for (var i = 0; i < length; i++)
        {
            chars[i] = (char)('0' + RandomNumberGenerator.GetInt32(0, 10));
        }

        return new string(chars);
    }

    private static MobileProviderDto BuildProvider(string code, string name)
    {
        var denominations = new[] { 10000m, 20000m, 50000m, 100000m, 200000m, 500000m };
        var products = new List<MobileProductDto>();

        products.AddRange(denominations.Select(amount => new MobileProductDto
        {
            Code = $"{code}_TOPUP_{(int)amount}",
            Name = $"Nap dien thoai {amount:N0} VND",
            ProductType = MobileTopUpProductType.PhoneTopUp.ToString(),
            Amount = amount
        }));

        products.AddRange(denominations.Select(amount => new MobileProductDto
        {
            Code = $"{code}_CARD_{(int)amount}",
            Name = $"Ma the {amount:N0} VND",
            ProductType = MobileTopUpProductType.CardCode.ToString(),
            Amount = amount
        }));

        products.AddRange(new[]
        {
            new MobileProductDto { Code = $"{code}_DATA_5GB", Name = "Goi data 5GB/30 ngay", ProductType = MobileTopUpProductType.DataPackage.ToString(), Amount = 70000m },
            new MobileProductDto { Code = $"{code}_DATA_10GB", Name = "Goi data 10GB/30 ngay", ProductType = MobileTopUpProductType.DataPackage.ToString(), Amount = 120000m },
            new MobileProductDto { Code = $"{code}_DATA_UNLIMITED", Name = "Goi data khong gioi han/30 ngay", ProductType = MobileTopUpProductType.DataPackage.ToString(), Amount = 200000m }
        });

        return new MobileProviderDto
        {
            Code = code,
            Name = name,
            Products = products
        };
    }

    [GeneratedRegex("[^0-9]")]
    private static partial Regex DigitsOnlyRegex();
}
