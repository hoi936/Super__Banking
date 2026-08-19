using System.Security.Cryptography;
using LocalLink.Application.Common.Exceptions;
using LocalLink.Application.Loans.DTOs;
using LocalLink.Application.Loans.Interfaces;
using LocalLink.Domain.Entities;
using LocalLink.Domain.Enums;
using LocalLink.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LocalLink.Infrastructure.Services;

public class LoanService : ILoanService
{
    private const decimal DefaultAnnualInterestRate = 12.5m;

    private readonly ApplicationDbContext _context;
    private readonly ILogger<LoanService> _logger;

    public LoanService(ApplicationDbContext context, ILogger<LoanService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<LoanApplicationDto>> GetMyApplicationsAsync(
        Guid currentUserId,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var customer = await GetCustomerAsync(currentUserId, cancellationToken);

        var query = _context.LoanApplications
            .AsNoTracking()
            .Include(loan => loan.Customer)
            .Include(loan => loan.DisbursementAccount)
            .Include(loan => loan.DisbursementTransaction)
            .Where(loan => loan.CustomerId == customer.Id);

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<LoanApplicationStatus>(status, true, out var parsedStatus))
            {
                throw new BadRequestException("Invalid loan application status.");
            }

            query = query.Where(loan => loan.Status == parsedStatus);
        }

        var applications = await query
            .OrderByDescending(loan => loan.SubmittedAtUtc)
            .ToListAsync(cancellationToken);

        return applications.Select(MapToDto).ToList();
    }

    public async Task<LoanApplicationDto> GetMyApplicationAsync(
        Guid currentUserId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var customer = await GetCustomerAsync(currentUserId, cancellationToken);

        var application = await _context.LoanApplications
            .AsNoTracking()
            .Include(loan => loan.Customer)
            .Include(loan => loan.DisbursementAccount)
            .Include(loan => loan.DisbursementTransaction)
            .FirstOrDefaultAsync(loan => loan.Id == id, cancellationToken);

        if (application == null || application.CustomerId != customer.Id)
        {
            throw new NotFoundException("Loan application", id);
        }

        return MapToDto(application);
    }

    public async Task<LoanApplicationDto> CreateApplicationAsync(
        Guid currentUserId,
        CreateLoanApplicationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.RequestedAmount <= 0)
        {
            throw new BadRequestException("Requested amount must be greater than zero.");
        }

        if (request.MonthlyIncome <= 0)
        {
            throw new BadRequestException("Monthly income must be greater than zero.");
        }

        var customer = await _context.Customers
            .Include(customer => customer.User)
            .FirstOrDefaultAsync(customer => customer.UserId == currentUserId, cancellationToken);

        if (customer == null)
        {
            throw new NotFoundException("Customer", currentUserId);
        }

        if (customer.Status != CustomerStatus.Active)
        {
            throw new BadRequestException("Customer profile is not active.");
        }

        var account = await _context.BankAccounts
            .FirstOrDefaultAsync(account => account.Id == request.DisbursementAccountId, cancellationToken);

        if (account == null || account.CustomerId != customer.Id)
        {
            throw new NotFoundException("Disbursement account not found or not owned by the current customer.");
        }

        if (account.Status != AccountStatus.Active)
        {
            throw new BadRequestException($"Disbursement account is {account.Status} and cannot receive loan proceeds.");
        }

        if (!string.Equals(account.Currency, "VND", StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestException("V2 consumer loans currently support VND accounts only.");
        }

        var now = DateTime.UtcNow;
        var application = new LoanApplication
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            DisbursementAccountId = account.Id,
            ApplicationNumber = GenerateReference("LOAN"),
            RequestedAmount = request.RequestedAmount,
            AnnualInterestRate = DefaultAnnualInterestRate,
            TermMonths = request.TermMonths,
            MonthlyIncome = request.MonthlyIncome,
            Purpose = request.Purpose.Trim(),
            Currency = account.Currency,
            Status = LoanApplicationStatus.Pending,
            SubmittedAtUtc = now,
            CreatedAtUtc = now
        };

        _context.LoanApplications.Add(application);
        _context.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = currentUserId,
            Action = "LOAN_APPLICATION_SUBMITTED",
            EntityType = "LoanApplication",
            EntityId = application.Id.ToString(),
            Description = $"Submitted loan application {application.ApplicationNumber} for {application.RequestedAmount:N2} {application.Currency}.",
            CreatedAtUtc = now
        });

        _context.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = currentUserId,
            Title = "Ho so vay von da duoc ghi nhan",
            Message = $"Ho so {application.ApplicationNumber} tri gia {application.RequestedAmount:N0} {application.Currency} dang cho duyet.",
            Type = NotificationType.Account,
            IsRead = false,
            CreatedAtUtc = now
        });

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Loan application {ApplicationNumber} submitted by customer {CustomerId}", application.ApplicationNumber, customer.Id);

        application.Customer = customer;
        application.DisbursementAccount = account;
        return MapToDto(application);
    }

    private async Task<Customer> GetCustomerAsync(Guid currentUserId, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(customer => customer.UserId == currentUserId, cancellationToken);

        if (customer == null)
        {
            throw new NotFoundException("Customer", currentUserId);
        }

        return customer;
    }

    internal static LoanApplicationDto MapToDto(LoanApplication application)
    {
        return new LoanApplicationDto
        {
            Id = application.Id,
            ApplicationNumber = application.ApplicationNumber,
            CustomerId = application.CustomerId,
            CustomerCode = application.Customer?.CustomerCode ?? string.Empty,
            CustomerFullName = application.Customer?.FullName ?? string.Empty,
            DisbursementAccountId = application.DisbursementAccountId,
            DisbursementAccountNumber = application.DisbursementAccount?.AccountNumber ?? string.Empty,
            RequestedAmount = application.RequestedAmount,
            ApprovedAmount = application.ApprovedAmount,
            AnnualInterestRate = application.AnnualInterestRate,
            TermMonths = application.TermMonths,
            MonthlyIncome = application.MonthlyIncome,
            Purpose = application.Purpose,
            Currency = application.Currency,
            Status = application.Status.ToString(),
            ReviewNote = application.ReviewNote,
            DisbursementReference = application.DisbursementTransaction?.ReferenceNumber,
            SubmittedAtUtc = application.SubmittedAtUtc,
            ReviewedAtUtc = application.ReviewedAtUtc,
            DisbursedAtUtc = application.DisbursedAtUtc
        };
    }

    private static string GenerateReference(string prefix)
    {
        return $"{prefix}{DateTime.UtcNow:yyyyMMddHHmmss}{RandomNumberGenerator.GetInt32(1000, 9999)}";
    }
}
