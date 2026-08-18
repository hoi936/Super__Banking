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

namespace LocalLink.UnitTests.Transactions;

public class TransactionServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly TransactionService _service;
    private readonly Mock<ILogger<TransactionService>> _loggerMock;

    public TransactionServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<TransactionService>>();
        _service = new TransactionService(_context, _loggerMock.Object);
    }

    [Fact]
    public async Task GetMyTransactionsAsync_ReturnsOnlyOwnedAccountTransactions()
    {
        // Arrange
        var user1 = new User { Id = Guid.NewGuid(), Email = "c1@locallink.local" };
        var customer1 = new Customer { Id = Guid.NewGuid(), UserId = user1.Id, CustomerCode = "CUS01", FullName = "Cust 1" };
        var account1 = new BankAccount { Id = Guid.NewGuid(), CustomerId = customer1.Id, AccountNumber = "1111111111", Balance = 10000000m, Currency = "VND", Status = AccountStatus.Active };

        var user2 = new User { Id = Guid.NewGuid(), Email = "c2@locallink.local" };
        var customer2 = new Customer { Id = Guid.NewGuid(), UserId = user2.Id, CustomerCode = "CUS02", FullName = "Cust 2" };
        var account2 = new BankAccount { Id = Guid.NewGuid(), CustomerId = customer2.Id, AccountNumber = "2222222222", Balance = 5000000m, Currency = "VND", Status = AccountStatus.Active };

        var otherAccount = new BankAccount { Id = Guid.NewGuid(), CustomerId = Guid.NewGuid(), AccountNumber = "3333333333", Balance = 5000000m, Currency = "VND", Status = AccountStatus.Active };

        // Tx1: customer1 to customer2
        var tx1 = new Transaction
        {
            Id = Guid.NewGuid(),
            ReferenceNumber = "TRF001",
            TransactionType = TransactionType.Transfer,
            SourceAccountId = account1.Id,
            DestinationAccountId = account2.Id,
            Amount = 500000m,
            Currency = "VND",
            Status = TransactionStatus.Completed,
            CreatedAtUtc = DateTime.UtcNow
        };

        // Tx2: other to other
        var tx2 = new Transaction
        {
            Id = Guid.NewGuid(),
            ReferenceNumber = "TRF002",
            TransactionType = TransactionType.Transfer,
            SourceAccountId = otherAccount.Id,
            DestinationAccountId = otherAccount.Id,
            Amount = 100000m,
            Currency = "VND",
            Status = TransactionStatus.Completed,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.Users.AddRange(user1, user2);
        _context.Customers.AddRange(customer1, customer2);
        _context.BankAccounts.AddRange(account1, account2, otherAccount);
        _context.Transactions.AddRange(tx1, tx2);
        await _context.SaveChangesAsync();

        // Act - Customer 1 queries transactions
        var result = await _service.GetMyTransactionsAsync(user1.Id, 1, 10, null, null, null, null);

        // Assert
        result.Should().NotBeNull();
        result.TotalItems.Should().Be(1);
        result.Items.Should().HaveCount(1);
        result.Items[0].ReferenceNumber.Should().Be("TRF001");
    }

    [Fact]
    public async Task GetTransactionDetailAsync_WithOwnedTransaction_ReturnsDetail()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "c1@locallink.local" };
        var customer = new Customer { Id = Guid.NewGuid(), UserId = user.Id, CustomerCode = "CUS01", FullName = "Cust 1" };
        var account = new BankAccount { Id = Guid.NewGuid(), CustomerId = customer.Id, AccountNumber = "1111111111", Balance = 10000000m, Currency = "VND", Status = AccountStatus.Active };

        var tx = new Transaction
        {
            Id = Guid.NewGuid(),
            ReferenceNumber = "TRF001",
            TransactionType = TransactionType.Transfer,
            SourceAccountId = account.Id,
            Amount = 500000m,
            Currency = "VND",
            Status = TransactionStatus.Completed,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.Users.Add(user);
        _context.Customers.Add(customer);
        _context.BankAccounts.Add(account);
        _context.Transactions.Add(tx);
        await _context.SaveChangesAsync();

        // Act
        var detail = await _service.GetTransactionDetailAsync(tx.Id, user.Id);

        // Assert
        detail.Should().NotBeNull();
        detail.ReferenceNumber.Should().Be("TRF001");
        detail.Amount.Should().Be(500000m);
    }

    [Fact]
    public async Task GetTransactionDetailAsync_WithUnownedTransaction_ThrowsNotFoundException()
    {
        // Arrange
        var user1 = new User { Id = Guid.NewGuid() };
        var customer1 = new Customer { Id = Guid.NewGuid(), UserId = user1.Id, CustomerCode = "CUS01", FullName = "Cust 1" };

        var user2 = new User { Id = Guid.NewGuid() };
        var customer2 = new Customer { Id = Guid.NewGuid(), UserId = user2.Id, CustomerCode = "CUS02", FullName = "Cust 2" };
        var account2 = new BankAccount { Id = Guid.NewGuid(), CustomerId = customer2.Id, AccountNumber = "2222222222", Balance = 5000000m, Currency = "VND", Status = AccountStatus.Active };

        var tx = new Transaction
        {
            Id = Guid.NewGuid(),
            ReferenceNumber = "TRF-OTHER",
            TransactionType = TransactionType.Transfer,
            SourceAccountId = account2.Id,
            Amount = 500000m,
            Currency = "VND",
            Status = TransactionStatus.Completed,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.Users.AddRange(user1, user2);
        _context.Customers.AddRange(customer1, customer2);
        _context.BankAccounts.Add(account2);
        _context.Transactions.Add(tx);
        await _context.SaveChangesAsync();

        // Act & Assert - Customer 1 queries Customer 2's transaction
        var act = async () => await _service.GetTransactionDetailAsync(tx.Id, user1.Id);
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
