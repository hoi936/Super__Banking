using System.Security.Cryptography;
using LocalLink.Application.Common.Exceptions;
using LocalLink.Application.Common.Models;
using LocalLink.Application.Loans.DTOs;
using LocalLink.Application.Loans.Interfaces;
using LocalLink.Domain.Entities;
using LocalLink.Domain.Enums;
using LocalLink.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LocalLink.Infrastructure.Services.Admin;

public class AdminLoanService : IAdminLoanService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AdminLoanService> _logger;

    public AdminLoanService(ApplicationDbContext context, ILogger<AdminLoanService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PagedResult<LoanApplicationDto>> GetApplicationsAsync(
        int page = 1,
        int pageSize = 20,
        string? status = null,
        string? keyword = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var query = BaseQuery().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<LoanApplicationStatus>(status, true, out var parsedStatus))
            {
                throw new BadRequestException("Invalid loan application status.");
            }

            query = query.Where(loan => loan.Status == parsedStatus);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var cleanKeyword = keyword.Trim();
            query = query.Where(loan =>
                loan.ApplicationNumber.Contains(cleanKeyword)
                || loan.Customer.CustomerCode.Contains(cleanKeyword)
                || loan.Customer.FullName.Contains(cleanKeyword)
                || loan.DisbursementAccount.AccountNumber.Contains(cleanKeyword));
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(loan => loan.SubmittedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<LoanApplicationDto>
        {
            Items = items.Select(LoanService.MapToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<LoanApplicationDto> GetApplicationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var application = await BaseQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(loan => loan.Id == id, cancellationToken);

        if (application == null)
        {
            throw new NotFoundException("Loan application", id);
        }

        return LoanService.MapToDto(application);
    }

    public async Task<LoanApplicationDto> ApproveAsync(
        Guid id,
        ApproveLoanApplicationRequest request,
        Guid reviewerUserId,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        if (request.ApprovedAmount <= 0)
        {
            throw new BadRequestException("Approved amount must be greater than zero.");
        }

        var application = await _context.LoanApplications
            .Include(loan => loan.Customer)
            .Include(loan => loan.DisbursementAccount)
            .Include(loan => loan.DisbursementTransaction)
            .FirstOrDefaultAsync(loan => loan.Id == id, cancellationToken);

        if (application == null)
        {
            throw new NotFoundException("Loan application", id);
        }

        if (application.Status != LoanApplicationStatus.Pending)
        {
            throw new BadRequestException($"Only pending loan applications can be approved. Current status: {application.Status}.");
        }

        if (request.ApprovedAmount > application.RequestedAmount)
        {
            throw new BadRequestException("Approved amount cannot exceed requested amount.");
        }

        var account = application.DisbursementAccount;
        if (account.Status != AccountStatus.Active)
        {
            throw new BadRequestException($"Disbursement account is {account.Status} and cannot receive loan proceeds.");
        }

        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var dbTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var now = DateTime.UtcNow;
                account.Balance += request.ApprovedAmount;
                account.UpdatedAtUtc = now;

                var referenceNumber = GenerateReference("LND");
                var transaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    ReferenceNumber = referenceNumber,
                    TransactionType = TransactionType.LoanDisbursement,
                    DestinationAccountId = account.Id,
                    Amount = request.ApprovedAmount,
                    Currency = application.Currency,
                    Description = $"Loan disbursement for {application.ApplicationNumber}",
                    Status = TransactionStatus.Completed,
                    CreatedAtUtc = now,
                    CompletedAtUtc = now
                };
                _context.Transactions.Add(transaction);

                application.Status = LoanApplicationStatus.Disbursed;
                application.ApprovedAmount = request.ApprovedAmount;
                application.AnnualInterestRate = request.AnnualInterestRate;
                application.ReviewNote = request.ReviewNote?.Trim();
                application.ReviewedByUserId = reviewerUserId;
                application.ReviewedAtUtc = now;
                application.DisbursedAtUtc = now;
                application.DisbursementTransactionId = transaction.Id;
                application.UpdatedAtUtc = now;

                _context.AuditLogs.Add(new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = reviewerUserId,
                    Action = "LOAN_APPLICATION_DISBURSED",
                    EntityType = "LoanApplication",
                    EntityId = application.Id.ToString(),
                    Description = $"Approved and disbursed {request.ApprovedAmount:N2} {application.Currency} for {application.ApplicationNumber}. Reference: {referenceNumber}",
                    IpAddress = ipAddress,
                    CreatedAtUtc = now
                });

                _context.Notifications.Add(new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = application.Customer.UserId,
                    Title = "Ho so vay da duoc giai ngan",
                    Message = $"Ho so {application.ApplicationNumber} da duoc duyet va giai ngan {request.ApprovedAmount:N0} {application.Currency}.",
                    Type = NotificationType.Account,
                    IsRead = false,
                    CreatedAtUtc = now
                });

                await _context.SaveChangesAsync(cancellationToken);
                await dbTransaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Loan application {ApplicationNumber} disbursed with reference {Reference}", application.ApplicationNumber, referenceNumber);

                application.DisbursementTransaction = transaction;
                return LoanService.MapToDto(application);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await dbTransaction.RollbackAsync(cancellationToken);
                _logger.LogWarning(ex, "Concurrency conflict while approving loan application {LoanApplicationId}", id);
                throw new ConflictException("A concurrency conflict occurred while approving the loan application. Please refresh and try again.");
            }
            catch
            {
                await dbTransaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    public async Task<LoanApplicationDto> RejectAsync(
        Guid id,
        RejectLoanApplicationRequest request,
        Guid reviewerUserId,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        var application = await _context.LoanApplications
            .Include(loan => loan.Customer)
            .Include(loan => loan.DisbursementAccount)
            .Include(loan => loan.DisbursementTransaction)
            .FirstOrDefaultAsync(loan => loan.Id == id, cancellationToken);

        if (application == null)
        {
            throw new NotFoundException("Loan application", id);
        }

        if (application.Status != LoanApplicationStatus.Pending)
        {
            throw new BadRequestException($"Only pending loan applications can be rejected. Current status: {application.Status}.");
        }

        var now = DateTime.UtcNow;
        application.Status = LoanApplicationStatus.Rejected;
        application.ReviewNote = request.Reason.Trim();
        application.ReviewedByUserId = reviewerUserId;
        application.ReviewedAtUtc = now;
        application.UpdatedAtUtc = now;

        _context.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = reviewerUserId,
            Action = "LOAN_APPLICATION_REJECTED",
            EntityType = "LoanApplication",
            EntityId = application.Id.ToString(),
            Description = $"Rejected loan application {application.ApplicationNumber}. Reason: {application.ReviewNote}",
            IpAddress = ipAddress,
            CreatedAtUtc = now
        });

        _context.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = application.Customer.UserId,
            Title = "Ho so vay chua duoc phe duyet",
            Message = $"Ho so {application.ApplicationNumber} chua duoc phe duyet. Ly do: {application.ReviewNote}",
            Type = NotificationType.Account,
            IsRead = false,
            CreatedAtUtc = now
        });

        await _context.SaveChangesAsync(cancellationToken);
        return LoanService.MapToDto(application);
    }

    private IQueryable<LoanApplication> BaseQuery()
    {
        return _context.LoanApplications
            .Include(loan => loan.Customer)
            .Include(loan => loan.DisbursementAccount)
            .Include(loan => loan.DisbursementTransaction);
    }

    private static string GenerateReference(string prefix)
    {
        return $"{prefix}{DateTime.UtcNow:yyyyMMddHHmmss}{RandomNumberGenerator.GetInt32(1000, 9999)}";
    }
}
