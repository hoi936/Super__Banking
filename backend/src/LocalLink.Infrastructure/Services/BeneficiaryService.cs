using LocalLink.Application.Beneficiaries.DTOs;
using LocalLink.Application.Beneficiaries.Interfaces;
using LocalLink.Application.Common.Exceptions;
using LocalLink.Domain.Entities;
using LocalLink.Domain.Enums;
using LocalLink.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LocalLink.Infrastructure.Services;

public class BeneficiaryService : IBeneficiaryService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<BeneficiaryService> _logger;

    public BeneficiaryService(ApplicationDbContext context, ILogger<BeneficiaryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<BeneficiaryDto>> GetMyBeneficiariesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (customer == null)
        {
            throw new NotFoundException("Customer profile not found for the authenticated user.");
        }

        var beneficiaries = await _context.Beneficiaries
            .AsNoTracking()
            .Include(b => b.BeneficiaryAccount)
            .Where(b => b.CustomerId == customer.Id)
            .OrderByDescending(b => b.CreatedAtUtc)
            .Select(b => new BeneficiaryDto
            {
                Id = b.Id,
                AccountNumber = b.BeneficiaryAccount.AccountNumber,
                AccountName = b.BeneficiaryAccount.AccountName,
                Nickname = b.Nickname,
                CreatedAtUtc = b.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return beneficiaries;
    }

    public async Task<BeneficiaryDto> AddBeneficiaryAsync(
        Guid userId, 
        CreateBeneficiaryRequest request, 
        CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (customer == null)
        {
            throw new NotFoundException("Customer profile not found for the authenticated user.");
        }

        if (string.IsNullOrWhiteSpace(request.AccountNumber))
        {
            throw new BadRequestException("Account number is required.");
        }

        var cleanNumber = request.AccountNumber.Trim();

        // 1. Target account must exist
        var targetAccount = await _context.BankAccounts
            .AsNoTracking()
            .FirstOrDefaultAsync(ba => ba.AccountNumber == cleanNumber, cancellationToken);

        if (targetAccount == null)
        {
            throw new NotFoundException($"Bank account with number '{cleanNumber}' was not found.");
        }

        // 2. Target account must not be CLOSED
        if (targetAccount.Status == AccountStatus.Closed)
        {
            throw new BadRequestException("Cannot add a closed bank account as a beneficiary.");
        }

        // 3. Cannot add own account
        if (targetAccount.CustomerId == customer.Id)
        {
            throw new BadRequestException("Cannot add your own bank account as a beneficiary.");
        }

        // 4. Cannot add duplicate beneficiary
        var isDuplicate = await _context.Beneficiaries
            .AsNoTracking()
            .AnyAsync(b => b.CustomerId == customer.Id && b.BeneficiaryAccountId == targetAccount.Id, cancellationToken);

        if (isDuplicate)
        {
            throw new ConflictException($"Bank account '{cleanNumber}' is already in your beneficiaries list.");
        }

        var beneficiary = new Beneficiary
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            BeneficiaryAccountId = targetAccount.Id,
            Nickname = string.IsNullOrWhiteSpace(request.Nickname) ? null : request.Nickname.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.Beneficiaries.Add(beneficiary);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Beneficiary {BeneficiaryId} created for Customer {CustomerId} (Target Account: {AccountNumber})",
            beneficiary.Id, customer.Id, cleanNumber);

        return new BeneficiaryDto
        {
            Id = beneficiary.Id,
            AccountNumber = targetAccount.AccountNumber,
            AccountName = targetAccount.AccountName,
            Nickname = beneficiary.Nickname,
            CreatedAtUtc = beneficiary.CreatedAtUtc
        };
    }

    public async Task DeleteBeneficiaryAsync(Guid userId, Guid beneficiaryId, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (customer == null)
        {
            throw new NotFoundException("Customer profile not found for the authenticated user.");
        }

        // Ownership enforcement: Only the owner can delete their beneficiary record
        var beneficiary = await _context.Beneficiaries
            .FirstOrDefaultAsync(b => b.Id == beneficiaryId && b.CustomerId == customer.Id, cancellationToken);

        if (beneficiary == null)
        {
            throw new NotFoundException("Beneficiary", beneficiaryId);
        }

        _context.Beneficiaries.Remove(beneficiary);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Beneficiary {BeneficiaryId} deleted by Customer {CustomerId}", beneficiaryId, customer.Id);
    }
}
