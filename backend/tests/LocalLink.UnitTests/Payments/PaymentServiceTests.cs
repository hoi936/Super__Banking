using FluentAssertions;
using LocalLink.Application.Common.Exceptions;
using LocalLink.Application.Payments.DTOs;
using LocalLink.Domain.Entities;
using LocalLink.Domain.Enums;
using LocalLink.Infrastructure.Persistence;
using LocalLink.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LocalLink.UnitTests.Payments;

public class PaymentServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly PaymentService _service;
    private readonly Mock<ILogger<PaymentService>> _loggerMock;

    public PaymentServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<PaymentService>>();
        _service = new PaymentService(_context, _loggerMock.Object);
    }

    [Fact]
    public async Task PayBillAsync_SuccessfulPayment_DebitsAccount_UpdatesBill_CreatesPayment_Transaction_Audit_Notification()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "user@locallink.local", Status = UserStatus.Active };
        var customer = new Customer { Id = Guid.NewGuid(), UserId = user.Id, CustomerCode = "CUS000001", FullName = "Nguyen Van An" };
        var account = new BankAccount
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            AccountNumber = "1000000001",
            AccountName = "Checking",
            Balance = 25000000m,
            Currency = "VND",
            Status = AccountStatus.Active
        };
        var bill = new Bill
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            ProviderName = "Da Nang Electricity",
            BillType = BillType.Electricity,
            BillNumber = "ELEC-2026-0001",
            Amount = 850000m,
            DueDate = new DateOnly(2026, 8, 30),
            Status = BillStatus.Unpaid
        };

        _context.Users.Add(user);
        _context.Customers.Add(customer);
        _context.BankAccounts.Add(account);
        _context.Bills.Add(bill);
        await _context.SaveChangesAsync();

        var request = new CreatePaymentRequest
        {
            BillId = bill.Id,
            AccountId = account.Id
        };

        // Act
        var receipt = await _service.PayBillAsync(request, "KEY-PAY-001", user.Id, "127.0.0.1");

        // Assert
        receipt.Should().NotBeNull();
        receipt.Status.Should().Be("COMPLETED");
        receipt.Amount.Should().Be(850000m);
        receipt.Reference.Should().StartWith("PAY");

        // Check account balance: 25,000,000 - 850,000 = 24,150,000
        var updatedAccount = await _context.BankAccounts.FindAsync(account.Id);
        updatedAccount!.Balance.Should().Be(24150000m);

        // Check bill status: PAID
        var updatedBill = await _context.Bills.FindAsync(bill.Id);
        updatedBill!.Status.Should().Be(BillStatus.Paid);

        // Check payment record
        var payment = await _context.Payments.FirstOrDefaultAsync(p => p.BillId == bill.Id);
        payment.Should().NotBeNull();
        payment!.Amount.Should().Be(850000m);
        payment.Status.Should().Be(PaymentStatus.Completed);
        payment.IdempotencyKey.Should().Be("KEY-PAY-001");

        // Check transaction record
        var transaction = await _context.Transactions.FirstOrDefaultAsync(t => t.Id == payment.TransactionId);
        transaction.Should().NotBeNull();
        transaction!.TransactionType.Should().Be(TransactionType.Payment);
        transaction.Amount.Should().Be(850000m);
        transaction.SourceAccountId.Should().Be(account.Id);

        // Check AuditLog
        var audit = await _context.AuditLogs.FirstOrDefaultAsync(a => a.Action == "PAYMENT_COMPLETED");
        audit.Should().NotBeNull();
        audit!.UserId.Should().Be(user.Id);

        // Check Notification
        var notif = await _context.Notifications.FirstOrDefaultAsync(n => n.UserId == user.Id && n.Type == NotificationType.Payment);
        notif.Should().NotBeNull();
    }

    [Fact]
    public async Task PayBillAsync_InsufficientFunds_ThrowsBadRequestException_AndDoesNotMutateBalanceOrBill()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "user@locallink.local", Status = UserStatus.Active };
        var customer = new Customer { Id = Guid.NewGuid(), UserId = user.Id, CustomerCode = "CUS000001", FullName = "Nguyen Van An" };
        var account = new BankAccount
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            AccountNumber = "1000000001",
            Balance = 100000m, // Only 100k
            Currency = "VND",
            Status = AccountStatus.Active
        };
        var bill = new Bill
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            ProviderName = "Electricity",
            BillNumber = "ELEC-001",
            Amount = 850000m, // Needs 850k
            Status = BillStatus.Unpaid
        };

        _context.Users.Add(user);
        _context.Customers.Add(customer);
        _context.BankAccounts.Add(account);
        _context.Bills.Add(bill);
        await _context.SaveChangesAsync();

        var request = new CreatePaymentRequest { BillId = bill.Id, AccountId = account.Id };

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _service.PayBillAsync(request, null, user.Id));

        var updatedAccount = await _context.BankAccounts.FindAsync(account.Id);
        updatedAccount!.Balance.Should().Be(100000m);

        var updatedBill = await _context.Bills.FindAsync(bill.Id);
        updatedBill!.Status.Should().Be(BillStatus.Unpaid);
    }

    [Fact]
    public async Task PayBillAsync_AlreadyPaidBill_ThrowsBadRequestException()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "user@locallink.local", Status = UserStatus.Active };
        var customer = new Customer { Id = Guid.NewGuid(), UserId = user.Id, CustomerCode = "CUS000001", FullName = "Nguyen Van An" };
        var account = new BankAccount
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            AccountNumber = "1000000001",
            Balance = 10000000m,
            Currency = "VND",
            Status = AccountStatus.Active
        };
        var bill = new Bill
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            ProviderName = "Electricity",
            BillNumber = "ELEC-001",
            Amount = 500000m,
            Status = BillStatus.Paid // Already Paid
        };

        _context.Users.Add(user);
        _context.Customers.Add(customer);
        _context.BankAccounts.Add(account);
        _context.Bills.Add(bill);
        await _context.SaveChangesAsync();

        var request = new CreatePaymentRequest { BillId = bill.Id, AccountId = account.Id };

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _service.PayBillAsync(request, null, user.Id));
    }

    [Fact]
    public async Task PayBillAsync_UnownedBill_ThrowsNotFoundException()
    {
        // Arrange
        var user1 = new User { Id = Guid.NewGuid(), Email = "user1@locallink.local", Status = UserStatus.Active };
        var customer1 = new Customer { Id = Guid.NewGuid(), UserId = user1.Id, CustomerCode = "CUS000001" };
        var account1 = new BankAccount { Id = Guid.NewGuid(), CustomerId = customer1.Id, AccountNumber = "1000000001", Balance = 5000000m, Currency = "VND", Status = AccountStatus.Active };

        var user2 = new User { Id = Guid.NewGuid(), Email = "user2@locallink.local", Status = UserStatus.Active };
        var customer2 = new Customer { Id = Guid.NewGuid(), UserId = user2.Id, CustomerCode = "CUS000002" };
        var bill2 = new Bill { Id = Guid.NewGuid(), CustomerId = customer2.Id, Amount = 500000m, Status = BillStatus.Unpaid };

        _context.Users.AddRange(user1, user2);
        _context.Customers.AddRange(customer1, customer2);
        _context.BankAccounts.Add(account1);
        _context.Bills.Add(bill2);
        await _context.SaveChangesAsync();

        var request = new CreatePaymentRequest { BillId = bill2.Id, AccountId = account1.Id };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.PayBillAsync(request, null, user1.Id));
    }

    [Fact]
    public async Task PayBillAsync_LockedAccount_ThrowsBadRequestException()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "user@locallink.local", Status = UserStatus.Active };
        var customer = new Customer { Id = Guid.NewGuid(), UserId = user.Id, CustomerCode = "CUS000001" };
        var account = new BankAccount
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            AccountNumber = "1000000001",
            Balance = 10000000m,
            Currency = "VND",
            Status = AccountStatus.Locked // Locked
        };
        var bill = new Bill { Id = Guid.NewGuid(), CustomerId = customer.Id, Amount = 500000m, Status = BillStatus.Unpaid };

        _context.Users.Add(user);
        _context.Customers.Add(customer);
        _context.BankAccounts.Add(account);
        _context.Bills.Add(bill);
        await _context.SaveChangesAsync();

        var request = new CreatePaymentRequest { BillId = bill.Id, AccountId = account.Id };

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _service.PayBillAsync(request, null, user.Id));
    }

    [Fact]
    public async Task PayBillAsync_IdempotentDuplicateReplay_ReturnsOriginalReceipt_WithoutDoubleDebit()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "user@locallink.local", Status = UserStatus.Active };
        var customer = new Customer { Id = Guid.NewGuid(), UserId = user.Id, CustomerCode = "CUS000001" };
        var account = new BankAccount { Id = Guid.NewGuid(), CustomerId = customer.Id, AccountNumber = "1000000001", Balance = 10000000m, Currency = "VND", Status = AccountStatus.Active };
        var bill = new Bill { Id = Guid.NewGuid(), CustomerId = customer.Id, ProviderName = "Internet", BillNumber = "NET-001", Amount = 500000m, Status = BillStatus.Unpaid };

        _context.Users.Add(user);
        _context.Customers.Add(customer);
        _context.BankAccounts.Add(account);
        _context.Bills.Add(bill);
        await _context.SaveChangesAsync();

        var request = new CreatePaymentRequest { BillId = bill.Id, AccountId = account.Id };

        // First execution
        var receipt1 = await _service.PayBillAsync(request, "IDEMPOTENT-PAY-KEY", user.Id);
        receipt1.Should().NotBeNull();

        // Second execution with same key and payload
        var receipt2 = await _service.PayBillAsync(request, "IDEMPOTENT-PAY-KEY", user.Id);
        receipt2.Should().NotBeNull();
        receipt2.PaymentId.Should().Be(receipt1.PaymentId);
        receipt2.Reference.Should().Be(receipt1.Reference);

        // Assert balance deducted ONLY once: 10,000,000 - 500,000 = 9,500,000
        var updatedAccount = await _context.BankAccounts.FindAsync(account.Id);
        updatedAccount!.Balance.Should().Be(9500000m);
    }

    [Fact]
    public async Task PayBillAsync_IdempotentConflict_ThrowsConflictException()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "user@locallink.local", Status = UserStatus.Active };
        var customer = new Customer { Id = Guid.NewGuid(), UserId = user.Id, CustomerCode = "CUS000001" };
        var account1 = new BankAccount { Id = Guid.NewGuid(), CustomerId = customer.Id, AccountNumber = "1000000001", Balance = 10000000m, Currency = "VND", Status = AccountStatus.Active };
        var account2 = new BankAccount { Id = Guid.NewGuid(), CustomerId = customer.Id, AccountNumber = "1000000002", Balance = 10000000m, Currency = "VND", Status = AccountStatus.Active };
        var bill = new Bill { Id = Guid.NewGuid(), CustomerId = customer.Id, Amount = 500000m, Status = BillStatus.Unpaid };

        _context.Users.Add(user);
        _context.Customers.Add(customer);
        _context.BankAccounts.AddRange(account1, account2);
        _context.Bills.Add(bill);
        await _context.SaveChangesAsync();

        var request1 = new CreatePaymentRequest { BillId = bill.Id, AccountId = account1.Id };
        var request2 = new CreatePaymentRequest { BillId = bill.Id, AccountId = account2.Id }; // Different account

        // First execution
        await _service.PayBillAsync(request1, "IDEMPOTENT-CONFLICT-KEY", user.Id);

        // Second execution with same key but different account
        await Assert.ThrowsAsync<ConflictException>(() => _service.PayBillAsync(request2, "IDEMPOTENT-CONFLICT-KEY", user.Id));
    }
}
