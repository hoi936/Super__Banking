using FluentAssertions;
using LocalLink.Application.Beneficiaries.DTOs;
using LocalLink.Application.Common.Exceptions;
using LocalLink.Domain.Entities;
using LocalLink.Domain.Enums;
using LocalLink.Infrastructure.Persistence;
using LocalLink.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LocalLink.UnitTests.Beneficiaries;

public class BeneficiaryServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly BeneficiaryService _service;
    private readonly Mock<ILogger<BeneficiaryService>> _loggerMock;

    public BeneficiaryServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<BeneficiaryService>>();
        _service = new BeneficiaryService(_context, _loggerMock.Object);
    }

    [Fact]
    public async Task AddBeneficiaryAsync_WithValidTargetAccount_CreatesBeneficiary()
    {
        // Arrange
        var user1 = new User { Id = Guid.NewGuid(), Email = "user1@locallink.local" };
        var customer1 = new Customer { Id = Guid.NewGuid(), UserId = user1.Id, FullName = "Customer 1" };
        var account1 = new BankAccount { Id = Guid.NewGuid(), CustomerId = customer1.Id, AccountNumber = "1000000001", Status = AccountStatus.Active };

        var user2 = new User { Id = Guid.NewGuid(), Email = "user2@locallink.local" };
        var customer2 = new Customer { Id = Guid.NewGuid(), UserId = user2.Id, FullName = "Customer 2" };
        var account2 = new BankAccount { Id = Guid.NewGuid(), CustomerId = customer2.Id, AccountNumber = "1000000002", AccountName = "Tran Thi Binh", Status = AccountStatus.Active };

        _context.Users.AddRange(user1, user2);
        _context.Customers.AddRange(customer1, customer2);
        _context.BankAccounts.AddRange(account1, account2);
        await _context.SaveChangesAsync();

        var request = new CreateBeneficiaryRequest
        {
            AccountNumber = "1000000002",
            Nickname = "Binh"
        };

        // Act
        var result = await _service.AddBeneficiaryAsync(user1.Id, request);

        // Assert
        result.Should().NotBeNull();
        result.AccountNumber.Should().Be("1000000002");
        result.AccountName.Should().Be("Tran Thi Binh");
        result.Nickname.Should().Be("Binh");

        var count = await _context.Beneficiaries.CountAsync(b => b.CustomerId == customer1.Id);
        count.Should().Be(1);
    }

    [Fact]
    public async Task AddBeneficiaryAsync_WithOwnAccount_ThrowsBadRequestException()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "user@locallink.local" };
        var customer = new Customer { Id = Guid.NewGuid(), UserId = user.Id };
        var account = new BankAccount { Id = Guid.NewGuid(), CustomerId = customer.Id, AccountNumber = "1000000001", Status = AccountStatus.Active };

        _context.Users.Add(user);
        _context.Customers.Add(customer);
        _context.BankAccounts.Add(account);
        await _context.SaveChangesAsync();

        var request = new CreateBeneficiaryRequest { AccountNumber = "1000000001" };

        // Act & Assert
        var act = async () => await _service.AddBeneficiaryAsync(user.Id, request);
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Cannot add your own bank account as a beneficiary.");
    }

    [Fact]
    public async Task AddBeneficiaryAsync_WithDuplicateAccount_ThrowsConflictException()
    {
        // Arrange
        var user1 = new User { Id = Guid.NewGuid(), Email = "user1@locallink.local" };
        var customer1 = new Customer { Id = Guid.NewGuid(), UserId = user1.Id };
        var account1 = new BankAccount { Id = Guid.NewGuid(), CustomerId = customer1.Id, AccountNumber = "1000000001", Status = AccountStatus.Active };

        var user2 = new User { Id = Guid.NewGuid(), Email = "user2@locallink.local" };
        var customer2 = new Customer { Id = Guid.NewGuid(), UserId = user2.Id };
        var account2 = new BankAccount { Id = Guid.NewGuid(), CustomerId = customer2.Id, AccountNumber = "1000000002", Status = AccountStatus.Active };

        var existingBeneficiary = new Beneficiary
        {
            Id = Guid.NewGuid(),
            CustomerId = customer1.Id,
            BeneficiaryAccountId = account2.Id
        };

        _context.Users.AddRange(user1, user2);
        _context.Customers.AddRange(customer1, customer2);
        _context.BankAccounts.AddRange(account1, account2);
        _context.Beneficiaries.Add(existingBeneficiary);
        await _context.SaveChangesAsync();

        var request = new CreateBeneficiaryRequest { AccountNumber = "1000000002" };

        // Act & Assert
        var act = async () => await _service.AddBeneficiaryAsync(user1.Id, request);
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task DeleteBeneficiaryAsync_WithUnownedBeneficiary_ThrowsNotFoundException()
    {
        // Arrange
        var user1 = new User { Id = Guid.NewGuid(), Email = "user1@locallink.local" };
        var customer1 = new Customer { Id = Guid.NewGuid(), UserId = user1.Id };

        var user2 = new User { Id = Guid.NewGuid(), Email = "user2@locallink.local" };
        var customer2 = new Customer { Id = Guid.NewGuid(), UserId = user2.Id };

        var beneficiary = new Beneficiary
        {
            Id = Guid.NewGuid(),
            CustomerId = customer2.Id,
            BeneficiaryAccountId = Guid.NewGuid()
        };

        _context.Users.AddRange(user1, user2);
        _context.Customers.AddRange(customer1, customer2);
        _context.Beneficiaries.Add(beneficiary);
        await _context.SaveChangesAsync();

        // Act & Assert - User1 deleting User2's beneficiary must throw NotFound
        var act = async () => await _service.DeleteBeneficiaryAsync(user1.Id, beneficiary.Id);
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
