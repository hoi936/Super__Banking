using FluentAssertions;
using LocalLink.Application.Common.Exceptions;
using LocalLink.Domain.Entities;
using LocalLink.Domain.Enums;
using LocalLink.Infrastructure.Persistence;
using LocalLink.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LocalLink.UnitTests.Accounts;

public class AccountServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly AccountService _service;
    private readonly Mock<ILogger<AccountService>> _loggerMock;

    public AccountServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<AccountService>>();
        _service = new AccountService(_context, _loggerMock.Object);
    }

    [Fact]
    public async Task GetMyAccountsAsync_ReturnsOnlyAccountsOwnedByCustomer()
    {
        // Arrange
        var user1 = new User { Id = Guid.NewGuid(), Email = "user1@locallink.local" };
        var customer1 = new Customer { Id = Guid.NewGuid(), UserId = user1.Id, FullName = "Customer One" };
        var account1 = new BankAccount
        {
            Id = Guid.NewGuid(),
            CustomerId = customer1.Id,
            AccountNumber = "1000000001",
            AccountName = "Customer One - Main",
            Balance = 25000000m,
            Status = AccountStatus.Active
        };

        var user2 = new User { Id = Guid.NewGuid(), Email = "user2@locallink.local" };
        var customer2 = new Customer { Id = Guid.NewGuid(), UserId = user2.Id, FullName = "Customer Two" };
        var account2 = new BankAccount
        {
            Id = Guid.NewGuid(),
            CustomerId = customer2.Id,
            AccountNumber = "1000000002",
            AccountName = "Customer Two - Main",
            Balance = 50000000m,
            Status = AccountStatus.Active
        };

        _context.Users.AddRange(user1, user2);
        _context.Customers.AddRange(customer1, customer2);
        _context.BankAccounts.AddRange(account1, account2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetMyAccountsAsync(user1.Id);

        // Assert
        result.Should().HaveCount(1);
        result[0].AccountNumber.Should().Be("1000000001");
        result[0].Balance.Should().Be(25000000m);
    }

    [Fact]
    public async Task GetMyAccountDetailAsync_WithOwnedAccount_ReturnsAccountDetailDto()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "user@locallink.local" };
        var customer = new Customer { Id = Guid.NewGuid(), UserId = user.Id, FullName = "Customer" };
        var account = new BankAccount
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            AccountNumber = "1000000001",
            AccountName = "Checking Account",
            AccountType = AccountType.Checking,
            Balance = 15000000m,
            Currency = "VND",
            Status = AccountStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.Users.Add(user);
        _context.Customers.Add(customer);
        _context.BankAccounts.Add(account);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetMyAccountDetailAsync(user.Id, account.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(account.Id);
        result.AccountNumber.Should().Be("1000000001");
        result.Balance.Should().Be(15000000m);
    }

    [Fact]
    public async Task GetMyAccountDetailAsync_WithOtherCustomerAccount_ThrowsNotFoundException()
    {
        // Arrange
        var user1 = new User { Id = Guid.NewGuid(), Email = "user1@locallink.local" };
        var customer1 = new Customer { Id = Guid.NewGuid(), UserId = user1.Id };

        var user2 = new User { Id = Guid.NewGuid(), Email = "user2@locallink.local" };
        var customer2 = new Customer { Id = Guid.NewGuid(), UserId = user2.Id };
        var account2 = new BankAccount
        {
            Id = Guid.NewGuid(),
            CustomerId = customer2.Id,
            AccountNumber = "1000000002"
        };

        _context.Users.AddRange(user1, user2);
        _context.Customers.AddRange(customer1, customer2);
        _context.BankAccounts.Add(account2);
        await _context.SaveChangesAsync();

        // Act & Assert - User1 trying to access User2's account must throw NotFound (404) to avoid data leakage
        var act = async () => await _service.GetMyAccountDetailAsync(user1.Id, account2.Id);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task LookupAccountAsync_WithActiveAccount_ReturnsMinimalLookupDto()
    {
        // Arrange
        var account = new BankAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "1000000002",
            AccountName = "Tran Thi Binh",
            Balance = 50000000m,
            Status = AccountStatus.Active
        };
        _context.BankAccounts.Add(account);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.LookupAccountAsync("1000000002");

        // Assert
        result.Should().NotBeNull();
        result.AccountNumber.Should().Be("1000000002");
        result.AccountName.Should().Be("Tran Thi Binh");
    }

    [Fact]
    public async Task LookupAccountAsync_WithClosedAccount_ThrowsNotFoundException()
    {
        // Arrange
        var account = new BankAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "1000000099",
            AccountName = "Closed Account",
            Status = AccountStatus.Closed
        };
        _context.BankAccounts.Add(account);
        await _context.SaveChangesAsync();

        // Act & Assert
        var act = async () => await _service.LookupAccountAsync("1000000099");
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
