using FluentAssertions;
using LocalLink.Application.Common.Exceptions;
using LocalLink.Application.Transfers.DTOs;
using LocalLink.Domain.Entities;
using LocalLink.Domain.Enums;
using LocalLink.Infrastructure.Persistence;
using LocalLink.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LocalLink.UnitTests.Transfers;

public class TransferServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly TransferService _service;
    private readonly Mock<ILogger<TransferService>> _loggerMock;

    public TransferServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<TransferService>>();
        _service = new TransferService(_context, _loggerMock.Object);
    }

    [Fact]
    public async Task TransferAsync_SuccessfulTransfer_DebitsSource_CreditsDestination_CreatesTransferAndLedgerAndAudit()
    {
        // Arrange
        var user1 = new User { Id = Guid.NewGuid(), Email = "cust1@locallink.local", Status = UserStatus.Active };
        var customer1 = new Customer { Id = Guid.NewGuid(), UserId = user1.Id, CustomerCode = "CUS000001", FullName = "Nguyen Van An" };
        var account1 = new BankAccount
        {
            Id = Guid.NewGuid(),
            CustomerId = customer1.Id,
            AccountNumber = "1000000001",
            AccountName = "Checking 1",
            Balance = 25000000m,
            Currency = "VND",
            Status = AccountStatus.Active
        };

        var user2 = new User { Id = Guid.NewGuid(), Email = "cust2@locallink.local", Status = UserStatus.Active };
        var customer2 = new Customer { Id = Guid.NewGuid(), UserId = user2.Id, CustomerCode = "CUS000002", FullName = "Tran Thi Binh" };
        var account2 = new BankAccount
        {
            Id = Guid.NewGuid(),
            CustomerId = customer2.Id,
            AccountNumber = "1000000002",
            AccountName = "Checking 2",
            Balance = 15000000m,
            Currency = "VND",
            Status = AccountStatus.Active
        };

        _context.Users.AddRange(user1, user2);
        _context.Customers.AddRange(customer1, customer2);
        _context.BankAccounts.AddRange(account1, account2);
        await _context.SaveChangesAsync();

        var request = new CreateTransferRequest
        {
            SourceAccountId = account1.Id,
            DestinationAccountNumber = "1000000002",
            Amount = 500000m,
            Description = "Chuyen tien an trua"
        };

        // Act
        var receipt = await _service.TransferAsync(request, "IDEM-001", user1.Id, "127.0.0.1");

        // Assert
        receipt.Should().NotBeNull();
        receipt.Amount.Should().Be(500000m);
        receipt.Status.Should().Be("COMPLETED");
        receipt.Reference.Should().StartWith("TRF");

        // Check balances
        var updatedAcc1 = await _context.BankAccounts.FirstAsync(ba => ba.Id == account1.Id);
        var updatedAcc2 = await _context.BankAccounts.FirstAsync(ba => ba.Id == account2.Id);
        updatedAcc1.Balance.Should().Be(24500000m);
        updatedAcc2.Balance.Should().Be(15500000m);

        // Check ledger & transfer record
        var transferRecord = await _context.Transfers.Include(tr => tr.Transaction).FirstAsync(tr => tr.Id == receipt.TransferId);
        transferRecord.Amount.Should().Be(500000m);
        transferRecord.IdempotencyKey.Should().Be("IDEM-001");
        transferRecord.Transaction.Should().NotBeNull();
        transferRecord.Transaction.TransactionType.Should().Be(TransactionType.Transfer);

        // Check Audit Log
        var audit = await _context.AuditLogs.FirstOrDefaultAsync(a => a.UserId == user1.Id && a.Action == "TRANSFER_COMPLETED");
        audit.Should().NotBeNull();
        audit!.EntityType.Should().Be("Transfer");
    }

    [Fact]
    public async Task TransferAsync_MoneyConservationInvariant_TotalBalanceIsPreserved()
    {
        // Arrange
        var user1 = new User { Id = Guid.NewGuid(), Email = "c1@locallink.local" };
        var customer1 = new Customer { Id = Guid.NewGuid(), UserId = user1.Id, CustomerCode = "CUS01", FullName = "Cust 1" };
        var account1 = new BankAccount { Id = Guid.NewGuid(), CustomerId = customer1.Id, AccountNumber = "1111111111", Balance = 25000000m, Currency = "VND", Status = AccountStatus.Active };

        var user2 = new User { Id = Guid.NewGuid(), Email = "c2@locallink.local" };
        var customer2 = new Customer { Id = Guid.NewGuid(), UserId = user2.Id, CustomerCode = "CUS02", FullName = "Cust 2" };
        var account2 = new BankAccount { Id = Guid.NewGuid(), CustomerId = customer2.Id, AccountNumber = "2222222222", Balance = 15000000m, Currency = "VND", Status = AccountStatus.Active };

        _context.Users.AddRange(user1, user2);
        _context.Customers.AddRange(customer1, customer2);
        _context.BankAccounts.AddRange(account1, account2);
        await _context.SaveChangesAsync();

        decimal totalBefore = account1.Balance + account2.Balance;

        var request = new CreateTransferRequest
        {
            SourceAccountId = account1.Id,
            DestinationAccountNumber = "2222222222",
            Amount = 1234567m
        };

        // Act
        await _service.TransferAsync(request, "IDEM-CONS", user1.Id, "127.0.0.1");

        // Assert
        var updatedAcc1 = await _context.BankAccounts.FirstAsync(ba => ba.Id == account1.Id);
        var updatedAcc2 = await _context.BankAccounts.FirstAsync(ba => ba.Id == account2.Id);
        decimal totalAfter = updatedAcc1.Balance + updatedAcc2.Balance;

        totalAfter.Should().Be(totalBefore, "Internal transfers must strictly conserve total money between source and destination");
    }

    [Fact]
    public async Task TransferAsync_InsufficientFunds_ThrowsBadRequestException_AndDoesNotMutateBalances()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid() };
        var customer = new Customer { Id = Guid.NewGuid(), UserId = user.Id, CustomerCode = "CUS01", FullName = "Cust" };
        var account1 = new BankAccount { Id = Guid.NewGuid(), CustomerId = customer.Id, AccountNumber = "1111111111", Balance = 1000000m, Currency = "VND", Status = AccountStatus.Active };
        var account2 = new BankAccount { Id = Guid.NewGuid(), CustomerId = Guid.NewGuid(), AccountNumber = "2222222222", Balance = 5000000m, Currency = "VND", Status = AccountStatus.Active };

        _context.Users.Add(user);
        _context.Customers.Add(customer);
        _context.BankAccounts.AddRange(account1, account2);
        await _context.SaveChangesAsync();

        var request = new CreateTransferRequest
        {
            SourceAccountId = account1.Id,
            DestinationAccountNumber = "2222222222",
            Amount = 2000000m // Exceeds balance
        };

        // Act & Assert
        var act = async () => await _service.TransferAsync(request, null, user.Id, "127.0.0.1");
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("*Insufficient funds*");

        var checkAcc1 = await _context.BankAccounts.FirstAsync(ba => ba.Id == account1.Id);
        var checkAcc2 = await _context.BankAccounts.FirstAsync(ba => ba.Id == account2.Id);
        checkAcc1.Balance.Should().Be(1000000m);
        checkAcc2.Balance.Should().Be(5000000m);
    }

    [Fact]
    public async Task TransferAsync_UnownedSourceAccount_ThrowsNotFoundException()
    {
        // Arrange
        var user1 = new User { Id = Guid.NewGuid() };
        var customer1 = new Customer { Id = Guid.NewGuid(), UserId = user1.Id, CustomerCode = "CUS01", FullName = "Cust 1" };

        var user2 = new User { Id = Guid.NewGuid() };
        var customer2 = new Customer { Id = Guid.NewGuid(), UserId = user2.Id, CustomerCode = "CUS02", FullName = "Cust 2" };
        var account2 = new BankAccount { Id = Guid.NewGuid(), CustomerId = customer2.Id, AccountNumber = "2222222222", Balance = 5000000m, Currency = "VND", Status = AccountStatus.Active };
        var destAccount = new BankAccount { Id = Guid.NewGuid(), CustomerId = Guid.NewGuid(), AccountNumber = "3333333333", Balance = 1000000m, Currency = "VND", Status = AccountStatus.Active };

        _context.Users.AddRange(user1, user2);
        _context.Customers.AddRange(customer1, customer2);
        _context.BankAccounts.AddRange(account2, destAccount);
        await _context.SaveChangesAsync();

        var request = new CreateTransferRequest
        {
            SourceAccountId = account2.Id, // Owned by Customer 2
            DestinationAccountNumber = "3333333333",
            Amount = 500000m
        };

        // Act & Assert - Customer 1 attempts to debit Customer 2's account
        var act = async () => await _service.TransferAsync(request, null, user1.Id, "127.0.0.1");
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task TransferAsync_LockedSourceAccount_ThrowsBadRequestException()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid() };
        var customer = new Customer { Id = Guid.NewGuid(), UserId = user.Id, CustomerCode = "CUS01", FullName = "Cust" };
        var account1 = new BankAccount { Id = Guid.NewGuid(), CustomerId = customer.Id, AccountNumber = "1111111111", Balance = 10000000m, Currency = "VND", Status = AccountStatus.Locked };
        var destAccount = new BankAccount { Id = Guid.NewGuid(), CustomerId = Guid.NewGuid(), AccountNumber = "2222222222", Balance = 1000000m, Currency = "VND", Status = AccountStatus.Active };

        _context.Users.Add(user);
        _context.Customers.Add(customer);
        _context.BankAccounts.AddRange(account1, destAccount);
        await _context.SaveChangesAsync();

        var request = new CreateTransferRequest
        {
            SourceAccountId = account1.Id,
            DestinationAccountNumber = "2222222222",
            Amount = 500000m
        };

        // Act & Assert
        var act = async () => await _service.TransferAsync(request, null, user.Id, "127.0.0.1");
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("*Locked*");
    }

    [Fact]
    public async Task TransferAsync_SameAccountTransfer_ThrowsBadRequestException()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid() };
        var customer = new Customer { Id = Guid.NewGuid(), UserId = user.Id, CustomerCode = "CUS01", FullName = "Cust" };
        var account = new BankAccount { Id = Guid.NewGuid(), CustomerId = customer.Id, AccountNumber = "1111111111", Balance = 10000000m, Currency = "VND", Status = AccountStatus.Active };

        _context.Users.Add(user);
        _context.Customers.Add(customer);
        _context.BankAccounts.Add(account);
        await _context.SaveChangesAsync();

        var request = new CreateTransferRequest
        {
            SourceAccountId = account.Id,
            DestinationAccountNumber = "1111111111", // Same account
            Amount = 500000m
        };

        // Act & Assert
        var act = async () => await _service.TransferAsync(request, null, user.Id, "127.0.0.1");
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("*Cannot transfer money to the same bank account*");
    }

    [Fact]
    public async Task TransferAsync_IdempotencyRetry_ReturnsOriginalReceipt_WithoutDoubleDebit()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid() };
        var customer = new Customer { Id = Guid.NewGuid(), UserId = user.Id, CustomerCode = "CUS01", FullName = "Cust" };
        var account1 = new BankAccount { Id = Guid.NewGuid(), CustomerId = customer.Id, AccountNumber = "1111111111", Balance = 10000000m, Currency = "VND", Status = AccountStatus.Active };

        var user2 = new User { Id = Guid.NewGuid() };
        var customer2 = new Customer { Id = Guid.NewGuid(), UserId = user2.Id, CustomerCode = "CUS02", FullName = "Cust 2" };
        var destAccount = new BankAccount { Id = Guid.NewGuid(), CustomerId = customer2.Id, AccountNumber = "2222222222", Balance = 5000000m, Currency = "VND", Status = AccountStatus.Active };

        _context.Users.AddRange(user, user2);
        _context.Customers.AddRange(customer, customer2);
        _context.BankAccounts.AddRange(account1, destAccount);
        await _context.SaveChangesAsync();

        var request = new CreateTransferRequest
        {
            SourceAccountId = account1.Id,
            DestinationAccountNumber = "2222222222",
            Amount = 500000m,
            Description = "Transfer 1"
        };

        // First attempt
        var receipt1 = await _service.TransferAsync(request, "IDEM-TEST-RETRY", user.Id, "127.0.0.1");
        receipt1.Amount.Should().Be(500000m);

        // Second attempt with exact same key and payload
        var receipt2 = await _service.TransferAsync(request, "IDEM-TEST-RETRY", user.Id, "127.0.0.1");

        // Assert
        receipt2.TransferId.Should().Be(receipt1.TransferId);
        receipt2.Reference.Should().Be(receipt1.Reference);

        var finalAcc1 = await _context.BankAccounts.FirstAsync(ba => ba.Id == account1.Id);
        var finalAcc2 = await _context.BankAccounts.FirstAsync(ba => ba.Id == destAccount.Id);
        finalAcc1.Balance.Should().Be(9500000m, "Only 500,000 must be debited once");
        finalAcc2.Balance.Should().Be(5500000m, "Only 500,000 must be credited once");
    }

    [Fact]
    public async Task TransferAsync_IdempotencyPayloadConflict_ThrowsConflictException()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid() };
        var customer = new Customer { Id = Guid.NewGuid(), UserId = user.Id, CustomerCode = "CUS01", FullName = "Cust" };
        var account1 = new BankAccount { Id = Guid.NewGuid(), CustomerId = customer.Id, AccountNumber = "1111111111", Balance = 10000000m, Currency = "VND", Status = AccountStatus.Active };

        var user2 = new User { Id = Guid.NewGuid() };
        var customer2 = new Customer { Id = Guid.NewGuid(), UserId = user2.Id, CustomerCode = "CUS02", FullName = "Cust 2" };
        var destAccount = new BankAccount { Id = Guid.NewGuid(), CustomerId = customer2.Id, AccountNumber = "2222222222", Balance = 5000000m, Currency = "VND", Status = AccountStatus.Active };

        _context.Users.AddRange(user, user2);
        _context.Customers.AddRange(customer, customer2);
        _context.BankAccounts.AddRange(account1, destAccount);
        await _context.SaveChangesAsync();

        var request1 = new CreateTransferRequest
        {
            SourceAccountId = account1.Id,
            DestinationAccountNumber = "2222222222",
            Amount = 500000m
        };

        // First attempt
        await _service.TransferAsync(request1, "IDEM-CONFLICT", user.Id, "127.0.0.1");

        // Second attempt with same key but DIFFERENT amount
        var request2 = new CreateTransferRequest
        {
            SourceAccountId = account1.Id,
            DestinationAccountNumber = "2222222222",
            Amount = 1000000m // Changed amount!
        };

        // Act & Assert
        var act = async () => await _service.TransferAsync(request2, "IDEM-CONFLICT", user.Id, "127.0.0.1");
        await act.Should().ThrowAsync<ConflictException>().WithMessage("*different transfer payload*");
    }
}
