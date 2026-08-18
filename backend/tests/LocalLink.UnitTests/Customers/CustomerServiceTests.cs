using FluentAssertions;
using LocalLink.Application.Common.Exceptions;
using LocalLink.Application.Customers.DTOs;
using LocalLink.Domain.Entities;
using LocalLink.Domain.Enums;
using LocalLink.Infrastructure.Persistence;
using LocalLink.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LocalLink.UnitTests.Customers;

public class CustomerServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly CustomerService _service;
    private readonly Mock<ILogger<CustomerService>> _loggerMock;

    public CustomerServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<CustomerService>>();
        _service = new CustomerService(_context, _loggerMock.Object);
    }

    [Fact]
    public async Task GetMyProfileAsync_WithValidCustomer_ReturnsProfileDto()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "user@locallink.local", Status = UserStatus.Active };
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            CustomerCode = "CUS000001",
            FullName = "Nguyen Van An",
            DateOfBirth = new DateOnly(1990, 1, 1),
            Gender = "Male",
            PhoneNumber = "0901234567",
            Address = "123 Le Loi, HCMC",
            Status = CustomerStatus.Active
        };

        _context.Users.Add(user);
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetMyProfileAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(customer.Id);
        result.CustomerCode.Should().Be("CUS000001");
        result.FullName.Should().Be("Nguyen Van An");
        result.Status.Should().Be("ACTIVE");
    }

    [Fact]
    public async Task GetMyProfileAsync_WithUnlinkedUser_ThrowsNotFoundException()
    {
        // Act & Assert
        var act = async () => await _service.GetMyProfileAsync(Guid.NewGuid());
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateMyProfileAsync_WithValidAllowedFields_UpdatesAndReturnsProfile()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "user@locallink.local", Status = UserStatus.Active };
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            CustomerCode = "CUS000001",
            FullName = "Old Name",
            Status = CustomerStatus.Active,
            CreatedAtUtc = DateTime.UtcNow.AddDays(-10)
        };

        _context.Users.Add(user);
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var request = new UpdateCustomerProfileRequest
        {
            FullName = "Nguyen Van An Updated",
            DateOfBirth = new DateOnly(1995, 5, 20),
            Gender = "Male",
            PhoneNumber = "0988776655",
            Address = "456 Nguyen Hue, HCMC"
        };

        // Act
        var result = await _service.UpdateMyProfileAsync(user.Id, request);

        // Assert
        result.FullName.Should().Be("Nguyen Van An Updated");
        result.PhoneNumber.Should().Be("0988776655");
        result.Address.Should().Be("456 Nguyen Hue, HCMC");

        // Verify protected fields were NOT modified
        var savedCustomer = await _context.Customers.FirstAsync(c => c.Id == customer.Id);
        savedCustomer.CustomerCode.Should().Be("CUS000001");
        savedCustomer.Status.Should().Be(CustomerStatus.Active);
        savedCustomer.UserId.Should().Be(user.Id);
        savedCustomer.UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateMyProfileAsync_WithEmptyFullName_ThrowsBadRequestException()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "user@locallink.local" };
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            CustomerCode = "CUS000001",
            FullName = "Old Name"
        };

        _context.Users.Add(user);
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var request = new UpdateCustomerProfileRequest { FullName = "   " };

        // Act & Assert
        var act = async () => await _service.UpdateMyProfileAsync(user.Id, request);
        await act.Should().ThrowAsync<BadRequestException>();
    }
}
