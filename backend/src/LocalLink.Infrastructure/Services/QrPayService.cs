using System.Globalization;
using LocalLink.Application.Common.Exceptions;
using LocalLink.Application.QrPay.DTOs;
using LocalLink.Application.QrPay.Interfaces;
using LocalLink.Domain.Enums;
using LocalLink.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LocalLink.Infrastructure.Services;

public class QrPayService : IQrPayService
{
    private const string LocalBankCode = "LOCAL_LINK";
    private const string LocalBankName = "SuperBanking LocalLink";

    private readonly ApplicationDbContext _context;
    private readonly ILogger<QrPayService> _logger;

    public QrPayService(ApplicationDbContext context, ILogger<QrPayService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<QrPayPayloadDto> CreateReceivePayloadAsync(
        Guid currentUserId,
        CreateQrPayloadRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Amount is <= 0)
        {
            throw new BadRequestException("QR amount must be greater than zero when provided.");
        }

        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(customer => customer.UserId == currentUserId, cancellationToken);

        if (customer == null)
        {
            throw new NotFoundException("Customer", currentUserId);
        }

        var account = await _context.BankAccounts
            .AsNoTracking()
            .FirstOrDefaultAsync(account => account.Id == request.AccountId, cancellationToken);

        if (account == null || account.CustomerId != customer.Id)
        {
            throw new NotFoundException("Account", request.AccountId);
        }

        if (account.Status != AccountStatus.Active)
        {
            throw new BadRequestException($"Account is {account.Status} and cannot receive QR payments.");
        }

        if (!string.Equals(account.Currency, "VND", StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestException("QR Pay currently supports VND accounts only.");
        }

        var now = DateTime.UtcNow;
        var accountName = string.IsNullOrWhiteSpace(account.AccountName) ? customer.FullName : account.AccountName;
        var payload = BuildPayload(account.AccountNumber, accountName, request.Amount, request.Description);

        _logger.LogInformation("QR Pay receive payload generated for account {AccountNumber}", account.AccountNumber);

        return new QrPayPayloadDto
        {
            Payload = payload,
            PayloadFormat = "LOCALBANK",
            AccountId = account.Id,
            AccountNumber = account.AccountNumber,
            AccountName = accountName,
            BankCode = LocalBankCode,
            BankName = LocalBankName,
            Amount = request.Amount,
            Description = NormalizeOptional(request.Description),
            GeneratedAtUtc = now
        };
    }

    public async Task<ParsedQrPayDto> ParsePayloadAsync(
        ParseQrPayloadRequest request,
        CancellationToken cancellationToken = default)
    {
        var rawPayload = request.Payload.Trim();
        if (string.IsNullOrWhiteSpace(rawPayload))
        {
            throw new BadRequestException("QR payload is required.");
        }

        var parsedValues = ParseUriPayload(rawPayload)
            ?? ParseDelimitedPayload(rawPayload)
            ?? ParseKeyValuePayload(rawPayload);

        if (parsedValues == null)
        {
            throw new BadRequestException("Unsupported QR payload format.");
        }

        var accountNumber = Pick(parsedValues, "account", "accountNumber", "acc", "stk", "toAccount");
        if (string.IsNullOrWhiteSpace(accountNumber))
        {
            throw new BadRequestException("QR payload does not contain an account number.");
        }

        var bankCode = Pick(parsedValues, "bank", "bankCode", "bin", "bankBin") ?? LocalBankCode;
        var bankName = Pick(parsedValues, "bankName", "bank") ?? ResolveBankName(bankCode);
        var accountName = Pick(parsedValues, "name", "accountName", "receiverName", "beneficiaryName");
        var amount = ParseAmount(Pick(parsedValues, "amount", "money", "transferAmount"));
        var description = NormalizeOptional(Pick(parsedValues, "content", "description", "addInfo", "message"));

        var isLocalBank = IsLocalBank(bankCode, bankName);
        var warning = default(string);

        if (isLocalBank)
        {
            var localAccount = await _context.BankAccounts
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    account => account.AccountNumber == accountNumber && account.Status != AccountStatus.Closed,
                    cancellationToken);

            if (localAccount == null)
            {
                warning = "LocalLink account from QR payload was not found.";
            }
            else
            {
                accountName = localAccount.AccountName;
                bankCode = LocalBankCode;
                bankName = LocalBankName;
            }
        }

        return new ParsedQrPayDto
        {
            IsSupported = true,
            PaymentRail = isLocalBank ? "LOCALBANK" : "VIETQR",
            BankCode = bankCode,
            BankName = bankName,
            AccountNumber = accountNumber,
            AccountName = NormalizeOptional(accountName),
            Amount = amount,
            Description = description,
            RawPayload = rawPayload,
            Warning = warning
        };
    }

    private static string BuildPayload(string accountNumber, string accountName, decimal? amount, string? description)
    {
        var query = new List<string>
        {
            $"bank={Uri.EscapeDataString(LocalBankCode)}",
            $"bankName={Uri.EscapeDataString(LocalBankName)}",
            $"account={Uri.EscapeDataString(accountNumber)}",
            $"name={Uri.EscapeDataString(accountName)}"
        };

        if (amount.HasValue)
        {
            query.Add($"amount={Uri.EscapeDataString(amount.Value.ToString("0.##", CultureInfo.InvariantCulture))}");
        }

        var cleanDescription = NormalizeOptional(description);
        if (cleanDescription != null)
        {
            query.Add($"content={Uri.EscapeDataString(cleanDescription)}");
        }

        return $"LOCALBANK://pay?{string.Join("&", query)}";
    }

    private static Dictionary<string, string>? ParseUriPayload(string rawPayload)
    {
        if (!Uri.TryCreate(rawPayload, UriKind.Absolute, out var uri))
        {
            return null;
        }

        var values = ParseQueryString(uri.Query);
        if (uri.Scheme.Equals("localbank", StringComparison.OrdinalIgnoreCase))
        {
            values.TryAdd("bank", LocalBankCode);
            return values;
        }

        if (uri.Host.Contains("vietqr", StringComparison.OrdinalIgnoreCase)
            || uri.AbsolutePath.Contains("vietqr", StringComparison.OrdinalIgnoreCase))
        {
            values.TryAdd("bankName", "VietQR");
            return values;
        }

        return values.Count > 0 ? values : null;
    }

    private static Dictionary<string, string>? ParseDelimitedPayload(string rawPayload)
    {
        var parts = rawPayload.Split('|', StringSplitOptions.TrimEntries);
        if (parts.Length < 2)
        {
            return null;
        }

        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["bank"] = parts.ElementAtOrDefault(0) ?? LocalBankCode,
            ["account"] = parts.ElementAtOrDefault(1) ?? string.Empty
        };

        AddIfNotBlank(result, "name", parts.ElementAtOrDefault(2));
        AddIfNotBlank(result, "amount", parts.ElementAtOrDefault(3));
        AddIfNotBlank(result, "content", parts.ElementAtOrDefault(4));

        return result;
    }

    private static Dictionary<string, string>? ParseKeyValuePayload(string rawPayload)
    {
        var lines = rawPayload
            .Split(['\n', '\r', ';', '&'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in lines)
        {
            var index = line.IndexOf('=');
            if (index <= 0 || index == line.Length - 1)
            {
                index = line.IndexOf(':');
            }

            if (index <= 0 || index >= line.Length - 1)
            {
                continue;
            }

            var key = line[..index].Trim();
            var value = Uri.UnescapeDataString(line[(index + 1)..].Trim());
            AddIfNotBlank(result, key, value);
        }

        return result.Count > 0 ? result : null;
    }

    private static Dictionary<string, string> ParseQueryString(string query)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var cleanQuery = query.TrimStart('?');
        if (string.IsNullOrWhiteSpace(cleanQuery))
        {
            return result;
        }

        foreach (var pair in cleanQuery.Split('&', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = pair.Split('=', 2);
            if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]))
            {
                continue;
            }

            result[Uri.UnescapeDataString(parts[0])] = Uri.UnescapeDataString(parts[1].Replace("+", " "));
        }

        return result;
    }

    private static string? Pick(Dictionary<string, string> values, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return null;
    }

    private static decimal? ParseAmount(string? rawAmount)
    {
        if (string.IsNullOrWhiteSpace(rawAmount))
        {
            return null;
        }

        var clean = rawAmount.Trim().Replace(" ", string.Empty).Replace("_", string.Empty);
        if (decimal.TryParse(clean, NumberStyles.Number, CultureInfo.InvariantCulture, out var invariantAmount))
        {
            return invariantAmount;
        }

        clean = clean.Replace(".", string.Empty).Replace(",", ".");
        return decimal.TryParse(clean, NumberStyles.Number, CultureInfo.InvariantCulture, out var localAmount)
            ? localAmount
            : null;
    }

    private static bool IsLocalBank(string bankCode, string bankName)
    {
        return bankCode.Equals(LocalBankCode, StringComparison.OrdinalIgnoreCase)
            || bankCode.Equals("LOCALBANK", StringComparison.OrdinalIgnoreCase)
            || bankName.Contains("LocalLink", StringComparison.OrdinalIgnoreCase)
            || bankName.Contains("SuperBanking", StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolveBankName(string bankCode)
    {
        return bankCode.ToUpperInvariant() switch
        {
            "970436" or "VCB" => "Vietcombank",
            "970416" or "ACB" => "ACB",
            "970407" or "TCB" => "Techcombank",
            "970418" or "BIDV" => "BIDV",
            "970415" or "VIB" => "VIB",
            "LOCALBANK" or LocalBankCode => LocalBankName,
            _ => bankCode
        };
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static void AddIfNotBlank(Dictionary<string, string> values, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            values[key] = value.Trim();
        }
    }
}
