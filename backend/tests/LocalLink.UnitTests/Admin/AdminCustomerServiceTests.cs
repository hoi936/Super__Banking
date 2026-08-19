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

namespace LocalLink.UnitTests.Admin;

public class AdminCustomerServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly AdminCustomerService _service;
    private readonly Mock<ILogger<AdminCustomerService>> _loggerMock;

    public AdminCustomerServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<AdminCustomerService>>();
        _service = new AdminCustomerService(_context, _loggerMock.Object);
    }

    [Fact]
    public async Task GetCustomersAsync_WithPaginationAndSearch_ReturnsPagedResult()
    {
        // Arrange
        var user1 = new User { Id = Guid.NewGuid(), Email = "customer1@locallink.local", Status = UserStatus.Active };
        var customer1 = new Customer
        {
            Id = Guid.NewGuid(),
            UserId = user1.Id,
            CustomerCode = "CUS000001",
            FullName = "Nguyen Van An",
            Status = CustomerStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        };

        var user2 = new User { Id = Guid.NewGuid(), Email = "customer2@locallink.local", Status = UserStatus.Active };
        var customer2 = new Customer
        {
            Id = Guid.NewGuid(),
            UserId = user2.Id,
            CustomerCode = "CUS000002",
            FullName = "Tran Thi Binh",
            Status = CustomerStatus.Active,
            CreatedAtUtc = DateTime.UtcNow.AddMinutes(1)
        };

        _context.Users.AddRange(user1, user2);
        _context.Customers.AddRange(customer1, customer2);
        await _context.SaveChangesAsync();

        // Act - Search for 'Tran'
        var result = await _service.GetCustomersAsync(1, 10, "Tran", null);

        // Assert
        result.Should().NotBeNull();
        result.TotalItems.Should().Be(1);
        result.Items.Should().HaveCount(1);
        result.Items[0].FullName.Should().Be("Tran Thi Binh");
    }

    [Fact]
    public async Task GetCustomerDetailAsync_WithValidCustomer_ReturnsFullDetail()
    {
        // Arrange
        var role = new Role { Id = Guid.NewGuid(), Name = "CUSTOMER" };
        var user = new User { Id = Guid.NewGuid(), Email = "customer1@locallink.local", Status = UserStatus.Active };
        user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id, Role = role });

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            CustomerCode = "CUS000001",
            FullName = "Nguyen Van An",
            Status = CustomerStatus.Active
        };
        customer.BankAccounts.Add(new BankAccount
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            AccountNumber = "1000000001",
            AccountName = "Checking",
            Balance = 25000000m,
            Status = AccountStatus.Active
        });

        _context.Roles.Add(role);
        _context.Users.Add(user);
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetCustomerDetailAsync(customer.Id);

        // Assert
        result.Should().NotBeNull();
        result.FullName.Should().Be("Nguyen Van An");
        result.Roles.Should().Contain("CUSTOMER");
        result.Accounts.Should().HaveCount(1);
        result.Accounts[0].AccountNumber.Should().Be("1000000001");
    }

    [Fact]
    public async Task UpdateCustomerStatusAsync_ToSuspended_UpdatesCustomerAndUser_AndCreatesAuditLog()
    {
        // Arrange
        var adminUserId = Guid.NewGuid();
        var user = new User { Id = Guid.NewGuid(), Email = "customer1@locallink.local", Status = UserStatus.Active };
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            CustomerCode = "CUS000001",
            FullName = "Nguyen Van An",
            Status = CustomerStatus.Active
        };

        _context.Users.Add(user);
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var request = new UpdateCustomerStatusRequest { Status = "SUSPENDED" };

        // Act
        var result = await _service.UpdateCustomerStatusAsync(customer.Id, request, adminUserId, "127.0.0.1");

        // Assert
        result.Status.Should().Be("SUSPENDED");

        var updatedCustomer = await _context.Customers.Include(c => c.User).FirstAsync(c => c.Id == customer.Id);
        updatedCustomer.Status.Should().Be(CustomerStatus.Suspended);
        updatedCustomer.User.Status.Should().Be(UserStatus.Suspended);

        // Verify Audit Log
        var audit = await _context.AuditLogs.FirstOrDefaultAsync(a => a.UserId == adminUserId && a.Action == "CUSTOMER_SUSPEND");
        audit.Should().NotBeNull();
        audit!.EntityType.Should().Be("Customer");
        audit.EntityId.Should().Be(customer.Id.ToString());
    }

    [Fact]
    public async Task UpdateCustomerStatusAsync_ToActive_ReactivatesCustomerAndUser()
    {
        // Arrange
        var adminUserId = Guid.NewGuid();
        var user = new User { Id = Guid.NewGuid(), Email = "customer1@locallink.local", Status = UserStatus.Suspended };
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            CustomerCode = "CUS000001",
            FullName = "Nguyen Van An",
            Status = CustomerStatus.Suspended
        };

        _context.Users.Add(user);
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var request = new UpdateCustomerStatusRequest { Status = "ACTIVE" };

        // Act
        var result = await _service.UpdateCustomerStatusAsync(customer.Id, request, adminUserId, "127.0.0.1");

        // Assert
        result.Status.Should().Be("ACTIVE");

        var updatedCustomer = await _context.Customers.Include(c => c.User).FirstAsync(c => c.Id == customer.Id);
        updatedCustomer.Status.Should().Be(CustomerStatus.Active);
        updatedCustomer.User.Status.Should().Be(UserStatus.Active);

        var audit = await _context.AuditLogs.FirstOrDefaultAsync(a => a.UserId == adminUserId && a.Action == "CUSTOMER_ACTIVATE");
        audit.Should().NotBeNull();
    }
}
