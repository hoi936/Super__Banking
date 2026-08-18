using LocalLink.Application.Common.Exceptions;
using LocalLink.Application.Customers.DTOs;
using LocalLink.Application.Customers.Interfaces;
using LocalLink.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LocalLink.Infrastructure.Services;

public class CustomerService : ICustomerService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(ApplicationDbContext context, ILogger<CustomerService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CustomerProfileDto> GetMyProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (customer == null)
        {
            throw new NotFoundException("Customer profile not found for the authenticated user.");
        }

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

    public async Task<CustomerProfileDto> UpdateMyProfileAsync(Guid userId, UpdateCustomerProfileRequest request, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (customer == null)
        {
            throw new NotFoundException("Customer profile not found for the authenticated user.");
        }

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            throw new BadRequestException("Full name is required.");
        }

        // Only update permitted fields
        customer.FullName = request.FullName.Trim();
        customer.DateOfBirth = request.DateOfBirth;
        customer.Gender = string.IsNullOrWhiteSpace(request.Gender) ? null : request.Gender.Trim();
        customer.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
        customer.Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim();
        customer.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Customer profile updated for UserId: {UserId}, CustomerId: {CustomerId}", userId, customer.Id);

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
