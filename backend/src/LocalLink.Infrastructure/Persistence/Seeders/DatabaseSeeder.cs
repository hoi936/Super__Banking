using LocalLink.Domain.Entities;
using LocalLink.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LocalLink.Infrastructure.Persistence.Seeders;

public static class DatabaseSeeder
{
    // Development placeholder hash (not plain text, temporary for M2 schema seeding before M3 Auth)
    private const string DevPasswordHashPlaceholder = "$2a$11$DevelopmentSeedPlaceholderHashDoNotUseInProduction1234567890.";

    public static async Task SeedAsync(ApplicationDbContext context, ILogger logger)
    {
        logger.LogInformation("Starting database seeding for development environment...");

        // 1. Seed Roles
        var rolesToSeed = new[]
        {
            new { Name = "CUSTOMER", Description = "Retail banking customer with access to banking services" },
            new { Name = "STAFF", Description = "Bank officer / teller with operational support access" },
            new { Name = "ADMIN", Description = "System administrator with platform governance access" }
        };

        var roleMap = new Dictionary<string, Role>(StringComparer.OrdinalIgnoreCase);

        foreach (var r in rolesToSeed)
        {
            var existingRole = await context.Roles.FirstOrDefaultAsync(x => x.Name == r.Name);
            if (existingRole == null)
            {
                var newRole = new Role
                {
                    Id = Guid.NewGuid(),
                    Name = r.Name,
                    Description = r.Description,
                    CreatedAtUtc = DateTime.UtcNow
                };
                context.Roles.Add(newRole);
                roleMap[r.Name] = newRole;
                logger.LogInformation("Seeded role: {RoleName}", r.Name);
            }
            else
            {
                roleMap[r.Name] = existingRole;
            }
        }

        await context.SaveChangesAsync();

        // 2. Seed Admin User
        var adminEmail = "admin@locallink.local";
        var existingAdmin = await context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Email == adminEmail);

        if (existingAdmin == null)
        {
            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                Email = adminEmail,
                PasswordHash = DevPasswordHashPlaceholder,
                Status = UserStatus.Active,
                CreatedAtUtc = DateTime.UtcNow
            };

            adminUser.UserRoles.Add(new UserRole
            {
                UserId = adminUser.Id,
                RoleId = roleMap["ADMIN"].Id,
                CreatedAtUtc = DateTime.UtcNow
            });

            context.Users.Add(adminUser);
            logger.LogInformation("Seeded admin user: {AdminEmail}", adminEmail);
        }

        // 3. Seed Customer 1: Nguyen Van An
        var cus1Email = "customer1@locallink.local";
        var cus1Code = "CUS000001";
        var cus1AccountNum = "1000000001";

        var existingUser1 = await context.Users
            .Include(u => u.Customer)
            .FirstOrDefaultAsync(u => u.Email == cus1Email);

        if (existingUser1 == null)
        {
            var user1 = new User
            {
                Id = Guid.NewGuid(),
                Email = cus1Email,
                PasswordHash = DevPasswordHashPlaceholder,
                Status = UserStatus.Active,
                CreatedAtUtc = DateTime.UtcNow
            };

            user1.UserRoles.Add(new UserRole
            {
                UserId = user1.Id,
                RoleId = roleMap["CUSTOMER"].Id,
                CreatedAtUtc = DateTime.UtcNow
            });

            var customer1 = new Customer
            {
                Id = Guid.NewGuid(),
                UserId = user1.Id,
                CustomerCode = cus1Code,
                FullName = "Nguyen Van An",
                DateOfBirth = new DateOnly(1990, 5, 15),
                Gender = "Male",
                PhoneNumber = "0901234567",
                Address = "123 Le Loi, District 1, Ho Chi Minh City",
                Status = CustomerStatus.Active,
                CreatedAtUtc = DateTime.UtcNow
            };

            var account1 = new BankAccount
            {
                Id = Guid.NewGuid(),
                CustomerId = customer1.Id,
                AccountNumber = cus1AccountNum,
                AccountName = "Nguyen Van An - Checking",
                AccountType = AccountType.Checking,
                Balance = 25000000.00m,
                Currency = "VND",
                Status = AccountStatus.Active,
                CreatedAtUtc = DateTime.UtcNow
            };

            customer1.BankAccounts.Add(account1);
            user1.Customer = customer1;

            context.Users.Add(user1);
            logger.LogInformation("Seeded customer 1: {FullName} ({Email}) with account {AccountNumber} (Balance: 25,000,000 VND)", 
                customer1.FullName, user1.Email, account1.AccountNumber);
        }

        // 4. Seed Customer 2: Tran Thi Binh
        var cus2Email = "customer2@locallink.local";
        var cus2Code = "CUS000002";
        var cus2AccountNum = "1000000002";

        var existingUser2 = await context.Users
            .Include(u => u.Customer)
            .FirstOrDefaultAsync(u => u.Email == cus2Email);

        if (existingUser2 == null)
        {
            var user2 = new User
            {
                Id = Guid.NewGuid(),
                Email = cus2Email,
                PasswordHash = DevPasswordHashPlaceholder,
                Status = UserStatus.Active,
                CreatedAtUtc = DateTime.UtcNow
            };

            user2.UserRoles.Add(new UserRole
            {
                UserId = user2.Id,
                RoleId = roleMap["CUSTOMER"].Id,
                CreatedAtUtc = DateTime.UtcNow
            });

            var customer2 = new Customer
            {
                Id = Guid.NewGuid(),
                UserId = user2.Id,
                CustomerCode = cus2Code,
                FullName = "Tran Thi Binh",
                DateOfBirth = new DateOnly(1995, 8, 20),
                Gender = "Female",
                PhoneNumber = "0912345678",
                Address = "456 Nguyen Hue, District 1, Ho Chi Minh City",
                Status = CustomerStatus.Active,
                CreatedAtUtc = DateTime.UtcNow
            };

            var account2 = new BankAccount
            {
                Id = Guid.NewGuid(),
                CustomerId = customer2.Id,
                AccountNumber = cus2AccountNum,
                AccountName = "Tran Thi Binh - Checking",
                AccountType = AccountType.Checking,
                Balance = 15000000.00m,
                Currency = "VND",
                Status = AccountStatus.Active,
                CreatedAtUtc = DateTime.UtcNow
            };

            customer2.BankAccounts.Add(account2);
            user2.Customer = customer2;

            context.Users.Add(user2);
            logger.LogInformation("Seeded customer 2: {FullName} ({Email}) with account {AccountNumber} (Balance: 15,000,000 VND)", 
                customer2.FullName, user2.Email, account2.AccountNumber);
        }

        await context.SaveChangesAsync();
        logger.LogInformation("Database seeding completed successfully.");
    }
}
