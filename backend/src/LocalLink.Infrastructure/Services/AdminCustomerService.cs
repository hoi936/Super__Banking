using LocalLink.Application.Accounts.DTOs;
using LocalLink.Application.Common.Exceptions;
using LocalLink.Application.Common.Models;
using LocalLink.Application.Customers.DTOs;
using LocalLink.Application.Customers.Interfaces;
using LocalLink.Domain.Entities;
using LocalLink.Domain.Enums;
using LocalLink.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LocalLink.Infrastructure.Services;

public class AdminCustomerService : IAdminCustomerService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AdminCustomerService> _logger;

    public AdminCustomerService(ApplicationDbContext context, ILogger<AdminCustomerService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PagedResult<AdminCustomerListItemDto>> GetCustomersAsync(
        int page, 
        int pageSize, 
        string? search, 
        string? status, 
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var query = _context.Customers
            .AsNoTracking()
            .Include(c => c.User)
            .Include(c => c.BankAccounts)
            .AsQueryable();

        // Apply Status Filter if provided
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (Enum.TryParse<CustomerStatus>(status, true, out var parsedStatus))
            {
                query = query.Where(c => c.Status == parsedStatus);
            }
        }

        // Apply Search Filter across CustomerCode, FullName, Email, PhoneNumber
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c => 
                c.CustomerCode.Contains(term) ||
                c.FullName.Contains(term) ||
                c.User.Email.Contains(term) ||
                (c.PhoneNumber != null && c.PhoneNumber.Contains(term)));
        }

        // Execute count at SQL level
        var totalItems = await query.CountAsync(cancellationToken);

        // Apply pagination and projection at SQL query level
        var items = await query
            .OrderByDescending(c => c.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new AdminCustomerListItemDto
            {
                Id = c.Id,
                UserId = c.UserId,
                Email = c.User.Email,
                CustomerCode = c.CustomerCode,
                FullName = c.FullName,
                PhoneNumber = c.PhoneNumber,
                CustomerStatus = c.Status.ToString().ToUpperInvariant(),
                UserStatus = c.User.Status.ToString().ToUpperInvariant(),
                AccountsCount = c.BankAccounts.Count,
                CreatedAtUtc = c.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<AdminCustomerListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<AdminCustomerDetailDto> GetCustomerDetailAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .Include(c => c.User)
                .ThenInclude(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
            .Include(c => c.BankAccounts)
            .FirstOrDefaultAsync(c => c.Id == customerId, cancellationToken);

        if (customer == null)
        {
            throw new NotFoundException("Customer", customerId);
        }

        return new AdminCustomerDetailDto
        {
            Id = customer.Id,
            UserId = customer.UserId,
            Email = customer.User.Email,
            CustomerCode = customer.CustomerCode,
            FullName = customer.FullName,
            DateOfBirth = customer.DateOfBirth,
            Gender = customer.Gender,
            PhoneNumber = customer.PhoneNumber,
            Address = customer.Address,
            CustomerStatus = customer.Status.ToString().ToUpperInvariant(),
            UserStatus = customer.User.Status.ToString().ToUpperInvariant(),
            Roles = customer.User.UserRoles.Select(ur => ur.Role.Name).ToList(),
            Accounts = customer.BankAccounts.Select(ba => new AccountSummaryDto
            {
                Id = ba.Id,
                AccountNumber = ba.AccountNumber,
                AccountName = ba.AccountName,
                AccountType = ba.AccountType.ToString().ToUpperInvariant(),
                Balance = ba.Balance,
                Currency = ba.Currency,
                Status = ba.Status.ToString().ToUpperInvariant()
            }).ToList(),
            CreatedAtUtc = customer.CreatedAtUtc,
            UpdatedAtUtc = customer.UpdatedAtUtc
        };
    }

    public async Task<CustomerProfileDto> UpdateCustomerStatusAsync(
        Guid customerId, 
        UpdateCustomerStatusRequest request, 
        Guid adminUserId, 
        string? ipAddress, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Status))
        {
            throw new BadRequestException("Status is required.");
        }

        if (!Enum.TryParse<CustomerStatus>(request.Status, true, out var newStatus))
        {
            throw new BadRequestException($"Invalid customer status: '{request.Status}'. Allowed values: ACTIVE, SUSPENDED.");
        }

        if (newStatus != CustomerStatus.Active && newStatus != CustomerStatus.Suspended)
        {
            throw new BadRequestException("Only ACTIVE and SUSPENDED transitions are permitted.");
        }

        var customer = await _context.Customers
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == customerId, cancellationToken);

        if (customer == null)
        {
            throw new NotFoundException("Customer", customerId);
        }

        if (customer.Status == CustomerStatus.Closed)
        {
            throw new BadRequestException("Cannot change status of a CLOSED customer profile.");
        }

        if (customer.Status == newStatus)
        {
            // Already in desired state, return current profile
            return new CustomerProfileDto
            {
                Id = customer.Id,
                CustomerCode = customer.CustomerCode,
                FullName = customer.FullName,
                DateOfBirth = customer.DateOfBirth,
                Gender = customer.Gender,
                PhoneNumber = customer.PhoneNumber,
                Address = customer.Address,
                Status = customer.Status.ToString().ToUpperInvariant()
            };
        }

        var oldStatus = customer.Status;
        customer.Status = newStatus;
        customer.UpdatedAtUtc = DateTime.UtcNow;

        // Synchronize linked user status
        if (newStatus == CustomerStatus.Suspended)
        {
            customer.User.Status = UserStatus.Suspended;
            customer.User.UpdatedAtUtc = DateTime.UtcNow;
        }
        else if (newStatus == CustomerStatus.Active)
        {
            customer.User.Status = UserStatus.Active;
            customer.User.UpdatedAtUtc = DateTime.UtcNow;
        }

        var action = newStatus == CustomerStatus.Suspended ? "CUSTOMER_SUSPEND" : "CUSTOMER_ACTIVATE";
        var audit = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = adminUserId,
            Action = action,
            EntityType = "Customer",
            EntityId = customer.Id.ToString(),
            Description = $"Admin changed customer status from {oldStatus} to {newStatus}",
            IpAddress = ipAddress,
            CreatedAtUtc = DateTime.UtcNow
        };
        _context.AuditLogs.Add(audit);

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Customer {CustomerId} status changed from {OldStatus} to {NewStatus} by Admin {AdminId}",
            customerId, oldStatus, newStatus, adminUserId);

        return new CustomerProfileDto
        {
            Id = customer.Id,
            CustomerCode = customer.CustomerCode,
            FullName = customer.FullName,
            DateOfBirth = customer.DateOfBirth,
            Gender = customer.Gender,
            PhoneNumber = customer.PhoneNumber,
            Address = customer.Address,
            Status = customer.Status.ToString().ToUpperInvariant()
        };
    }
}
