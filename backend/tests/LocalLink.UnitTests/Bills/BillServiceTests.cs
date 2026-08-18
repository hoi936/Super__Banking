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

namespace LocalLink.UnitTests.Bills;

public class BillServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly BillService _service;
    private readonly Mock<ILogger<BillService>> _loggerMock;

    public BillServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<BillService>>();
        _service = new BillService(_context, _loggerMock.Object);
    }

    [Fact]
    public async Task GetBillsAsync_ReturnsOnlyCurrentCustomerBills_WithPagination()
    {
        // Arrange
        var user1 = new User { Id = Guid.NewGuid(), Email = "user1@locallink.local", Status = UserStatus.Active };
        var customer1 = new Customer { Id = Guid.NewGuid(), UserId = user1.Id, CustomerCode = "CUS000001", FullName = "Nguyen Van An" };
        var bill1 = new Bill
        {
            Id = Guid.NewGuid(),
            CustomerId = customer1.Id,
            ProviderName = "Da Nang Electricity",
            BillType = BillType.Electricity,
            BillNumber = "ELEC-001",
            Amount = 850000m,
            DueDate = new DateOnly(2026, 8, 30),
            Status = BillStatus.Unpaid,
            CreatedAtUtc = DateTime.UtcNow
        };
        var bill2 = new Bill
        {
            Id = Guid.NewGuid(),
            CustomerId = customer1.Id,
            ProviderName = "Da Nang Water",
            BillType = BillType.Water,
            BillNumber = "WATER-001",
            Amount = 220000m,
            DueDate = new DateOnly(2026, 8, 30),
            Status = BillStatus.Unpaid,
            CreatedAtUtc = DateTime.UtcNow
        };

        var user2 = new User { Id = Guid.NewGuid(), Email = "user2@locallink.local", Status = UserStatus.Active };
        var customer2 = new Customer { Id = Guid.NewGuid(), UserId = user2.Id, CustomerCode = "CUS000002", FullName = "Tran Thi Binh" };
        var bill3 = new Bill
        {
            Id = Guid.NewGuid(),
            CustomerId = customer2.Id,
            ProviderName = "VNPT Internet",
            BillType = BillType.Internet,
            BillNumber = "NET-002",
            Amount = 350000m,
            DueDate = new DateOnly(2026, 8, 30),
            Status = BillStatus.Unpaid,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.Users.AddRange(user1, user2);
        _context.Customers.AddRange(customer1, customer2);
        _context.Bills.AddRange(bill1, bill2, bill3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetBillsAsync(user1.Id, page: 1, pageSize: 10);

        // Assert
        result.Should().NotBeNull();
        result.TotalItems.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.Items.Select(x => x.BillNumber).Should().Contain(new[] { "ELEC-001", "WATER-001" });
        result.Items.Select(x => x.BillNumber).Should().NotContain("NET-002");
    }

    [Fact]
    public async Task GetBillsAsync_StatusFilter_ReturnsOnlyMatchingStatus()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "user@locallink.local", Status = UserStatus.Active };
        var customer = new Customer { Id = Guid.NewGuid(), UserId = user.Id, CustomerCode = "CUS000001", FullName = "Nguyen Van An" };
        var unpaidBill = new Bill
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            ProviderName = "Electricity",
            BillType = BillType.Electricity,
            BillNumber = "ELEC-001",
            Amount = 500000m,
            DueDate = new DateOnly(2026, 8, 30),
            Status = BillStatus.Unpaid
        };
        var paidBill = new Bill
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            ProviderName = "Water",
            BillType = BillType.Water,
            BillNumber = "WATER-001",
            Amount = 200000m,
            DueDate = new DateOnly(2026, 8, 30),
            Status = BillStatus.Paid
        };

        _context.Users.Add(user);
        _context.Customers.Add(customer);
        _context.Bills.AddRange(unpaidBill, paidBill);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetBillsAsync(user.Id, status: "UNPAID");

        // Assert
        result.TotalItems.Should().Be(1);
        result.Items.First().BillNumber.Should().Be("ELEC-001");
    }

    [Fact]
    public async Task GetBillDetailAsync_ValidBill_ReturnsBillDetail()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "user@locallink.local", Status = UserStatus.Active };
        var customer = new Customer { Id = Guid.NewGuid(), UserId = user.Id, CustomerCode = "CUS000001", FullName = "Nguyen Van An" };
        var bill = new Bill
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            ProviderName = "Da Nang Electricity",
            BillType = BillType.Electricity,
            BillNumber = "ELEC-001",
            Amount = 850000m,
            DueDate = new DateOnly(2026, 8, 30),
            Status = BillStatus.Unpaid
        };

        _context.Users.Add(user);
        _context.Customers.Add(customer);
        _context.Bills.Add(bill);
        await _context.SaveChangesAsync();

        // Act
        var detail = await _service.GetBillDetailAsync(bill.Id, user.Id);

        // Assert
        detail.Should().NotBeNull();
        detail.Id.Should().Be(bill.Id);
        detail.BillNumber.Should().Be("ELEC-001");
        detail.Amount.Should().Be(850000m);
        detail.Status.Should().Be("UNPAID");
    }

    [Fact]
    public async Task GetBillDetailAsync_UnownedBill_ThrowsNotFoundException()
    {
        // Arrange
        var user1 = new User { Id = Guid.NewGuid(), Email = "user1@locallink.local", Status = UserStatus.Active };
        var customer1 = new Customer { Id = Guid.NewGuid(), UserId = user1.Id, CustomerCode = "CUS000001", FullName = "User One" };

        var user2 = new User { Id = Guid.NewGuid(), Email = "user2@locallink.local", Status = UserStatus.Active };
        var customer2 = new Customer { Id = Guid.NewGuid(), UserId = user2.Id, CustomerCode = "CUS000002", FullName = "User Two" };
        var bill2 = new Bill
        {
            Id = Guid.NewGuid(),
            CustomerId = customer2.Id,
            ProviderName = "Internet",
            BillType = BillType.Internet,
            BillNumber = "NET-002",
            Amount = 300000m,
            DueDate = new DateOnly(2026, 8, 30),
            Status = BillStatus.Unpaid
        };

        _context.Users.AddRange(user1, user2);
        _context.Customers.AddRange(customer1, customer2);
        _context.Bills.Add(bill2);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetBillDetailAsync(bill2.Id, user1.Id));
    }
}
