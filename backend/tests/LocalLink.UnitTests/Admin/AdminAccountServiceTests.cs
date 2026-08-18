using FluentAssertions;
using LocalLink.Application.Accounts.DTOs;
using LocalLink.Application.Common.Exceptions;
using LocalLink.Domain.Entities;
using LocalLink.Domain.Enums;
using LocalLink.Infrastructure.Persistence;
using LocalLink.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LocalLink.UnitTests.Admin;

public class AdminAccountServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly AdminAccountService _service;
    private readonly Mock<ILogger<AdminAccountService>> _loggerMock;

    public AdminAccountServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<AdminAccountService>>();
        _service = new AdminAccountService(_context, _loggerMock.Object);
    }

    [Fact]
    public async Task GetCustomerAccountsAsync_ReturnsAllAccountsOfCustomer()
    {
        // Arrange
        var customer = new Customer { Id = Guid.NewGuid(), CustomerCode = "CUS000001", FullName = "Customer" };
        var account1 = new BankAccount { Id = Guid.NewGuid(), CustomerId = customer.Id, AccountNumber = "1000000001", Balance = 25000000m, Status = AccountStatus.Active };
        var account2 = new BankAccount { Id = Guid.NewGuid(), CustomerId = customer.Id, AccountNumber = "1000000002", Balance = 10000000m, Status = AccountStatus.Active };

        _context.Customers.Add(customer);
        _context.BankAccounts.AddRange(account1, account2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetCustomerAccountsAsync(customer.Id);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateAccountStatusAsync_ToLocked_UpdatesStatusAndLogsAudit()
    {
        // Arrange
        var adminUserId = Guid.NewGuid();
        var account = new BankAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "1000000001",
            AccountName = "Checking",
            Balance = 25000000m,
            Status = AccountStatus.Active
        };

        _context.BankAccounts.Add(account);
        await _context.SaveChangesAsync();

        var request = new UpdateAccountStatusRequest { Status = "LOCKED" };

        // Act
        var result = await _service.UpdateAccountStatusAsync(account.Id, request, adminUserId, "127.0.0.1");

        // Assert
        result.Status.Should().Be("LOCKED");

        var updatedAccount = await _context.BankAccounts.FirstAsync(ba => ba.Id == account.Id);
        updatedAccount.Status.Should().Be(AccountStatus.Locked);

        var audit = await _context.AuditLogs.FirstOrDefaultAsync(a => a.UserId == adminUserId && a.Action == "ACCOUNT_LOCK");
        audit.Should().NotBeNull();
        audit!.EntityType.Should().Be("BankAccount");
        audit.EntityId.Should().Be(account.Id.ToString());
    }

    [Fact]
    public async Task UpdateAccountStatusAsync_ToActive_UnlocksAccount()
    {
        // Arrange
        var adminUserId = Guid.NewGuid();
        var account = new BankAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "1000000001",
            AccountName = "Checking",
            Balance = 25000000m,
            Status = AccountStatus.Locked
        };

        _context.BankAccounts.Add(account);
        await _context.SaveChangesAsync();

        var request = new UpdateAccountStatusRequest { Status = "ACTIVE" };

        // Act
        var result = await _service.UpdateAccountStatusAsync(account.Id, request, adminUserId, "127.0.0.1");

        // Assert
        result.Status.Should().Be("ACTIVE");

        var updatedAccount = await _context.BankAccounts.FirstAsync(ba => ba.Id == account.Id);
        updatedAccount.Status.Should().Be(AccountStatus.Active);

        var audit = await _context.AuditLogs.FirstOrDefaultAsync(a => a.UserId == adminUserId && a.Action == "ACCOUNT_UNLOCK");
        audit.Should().NotBeNull();
    }
}
