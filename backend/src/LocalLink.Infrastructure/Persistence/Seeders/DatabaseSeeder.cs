using LocalLink.Domain.Entities;
using LocalLink.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LocalLink.Infrastructure.Persistence.Seeders;

public static class DatabaseSeeder
{
    // Development-only demo credentials (never use in production)
    public const string DevDemoPassword = "LocalLink@123";

    public static async Task SeedAsync(ApplicationDbContext context, ILogger logger)
    {
        logger.LogInformation("Starting database seeding for development environment...");
        var passwordHasher = new PasswordHasher<User>();

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
                Status = UserStatus.Active,
                CreatedAtUtc = DateTime.UtcNow
            };
            adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, DevDemoPassword);

            adminUser.UserRoles.Add(new UserRole
            {
                UserId = adminUser.Id,
                RoleId = roleMap["ADMIN"].Id,
                CreatedAtUtc = DateTime.UtcNow
            });

            context.Users.Add(adminUser);
            logger.LogInformation("Seeded admin user: {AdminEmail}", adminEmail);
        }
        else if (passwordHasher.VerifyHashedPassword(existingAdmin, existingAdmin.PasswordHash, DevDemoPassword) == PasswordVerificationResult.Failed)
        {
            // Ensure demo hash is updated to real standard ASP.NET Core hash
            existingAdmin.PasswordHash = passwordHasher.HashPassword(existingAdmin, DevDemoPassword);
            logger.LogInformation("Updated password hash for admin user: {AdminEmail}", adminEmail);
        }

        // 2.5. Seed Staff User
        var staffEmail = "staff@locallink.local";
        var existingStaff = await context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Email == staffEmail);

        if (existingStaff == null)
        {
            var staffUser = new User
            {
                Id = Guid.NewGuid(),
                Email = staffEmail,
                Status = UserStatus.Active,
                CreatedAtUtc = DateTime.UtcNow
            };
            staffUser.PasswordHash = passwordHasher.HashPassword(staffUser, DevDemoPassword);

            staffUser.UserRoles.Add(new UserRole
            {
                UserId = staffUser.Id,
                RoleId = roleMap["STAFF"].Id,
                CreatedAtUtc = DateTime.UtcNow
            });

            context.Users.Add(staffUser);
            logger.LogInformation("Seeded staff user: {StaffEmail}", staffEmail);
        }
        else if (passwordHasher.VerifyHashedPassword(existingStaff, existingStaff.PasswordHash, DevDemoPassword) == PasswordVerificationResult.Failed)
        {
            existingStaff.PasswordHash = passwordHasher.HashPassword(existingStaff, DevDemoPassword);
            logger.LogInformation("Updated password hash for staff user: {StaffEmail}", staffEmail);
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
                Status = UserStatus.Active,
                CreatedAtUtc = DateTime.UtcNow
            };
            user1.PasswordHash = passwordHasher.HashPassword(user1, DevDemoPassword);

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
        else if (passwordHasher.VerifyHashedPassword(existingUser1, existingUser1.PasswordHash, DevDemoPassword) == PasswordVerificationResult.Failed)
        {
            existingUser1.PasswordHash = passwordHasher.HashPassword(existingUser1, DevDemoPassword);
            logger.LogInformation("Updated password hash for customer 1: {Email}", cus1Email);
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
                Status = UserStatus.Active,
                CreatedAtUtc = DateTime.UtcNow
            };
            user2.PasswordHash = passwordHasher.HashPassword(user2, DevDemoPassword);

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
        else if (passwordHasher.VerifyHashedPassword(existingUser2, existingUser2.PasswordHash, DevDemoPassword) == PasswordVerificationResult.Failed)
        {
            existingUser2.PasswordHash = passwordHasher.HashPassword(existingUser2, DevDemoPassword);
            logger.LogInformation("Updated password hash for customer 2: {Email}", cus2Email);
        }

        await context.SaveChangesAsync();

        // 5. Seed Demo Bills
        var cus1 = await context.Customers.FirstOrDefaultAsync(c => c.CustomerCode == "CUS000001");
        if (cus1 != null)
        {
            if (!await context.Bills.AnyAsync(b => b.BillNumber == "ELEC-2026-0001"))
            {
                context.Bills.Add(new Bill
                {
                    Id = Guid.NewGuid(),
                    CustomerId = cus1.Id,
                    ProviderName = "Da Nang Electricity",
                    BillType = BillType.Electricity,
                    BillNumber = "ELEC-2026-0001",
                    Amount = 850000.00m,
                    DueDate = new DateOnly(2026, 8, 30),
                    Status = BillStatus.Unpaid,
                    CreatedAtUtc = DateTime.UtcNow
                });
                logger.LogInformation("Seeded demo bill ELEC-2026-0001 for Customer 1 (850,000 VND)");
            }

            if (!await context.Bills.AnyAsync(b => b.BillNumber == "WATER-2026-0001"))
            {
                context.Bills.Add(new Bill
                {
                    Id = Guid.NewGuid(),
                    CustomerId = cus1.Id,
                    ProviderName = "Da Nang Water",
                    BillType = BillType.Water,
                    BillNumber = "WATER-2026-0001",
                    Amount = 220000.00m,
                    DueDate = new DateOnly(2026, 8, 30),
                    Status = BillStatus.Unpaid,
                    CreatedAtUtc = DateTime.UtcNow
                });
                logger.LogInformation("Seeded demo bill WATER-2026-0001 for Customer 1 (220,000 VND)");
            }
        }

        var cus2 = await context.Customers.FirstOrDefaultAsync(c => c.CustomerCode == "CUS000002");
        if (cus2 != null)
        {
            if (!await context.Bills.AnyAsync(b => b.BillNumber == "NET-2026-0002"))
            {
                context.Bills.Add(new Bill
                {
                    Id = Guid.NewGuid(),
                    CustomerId = cus2.Id,
                    ProviderName = "VNPT Internet",
                    BillType = BillType.Internet,
                    BillNumber = "NET-2026-0002",
                    Amount = 350000.00m,
                    DueDate = new DateOnly(2026, 8, 30),
                    Status = BillStatus.Unpaid,
                    CreatedAtUtc = DateTime.UtcNow
                });
                logger.LogInformation("Seeded demo bill NET-2026-0002 for Customer 2 (350,000 VND)");
            }
        }

        await context.SaveChangesAsync();
        
        // 6. Bulk Data Generation (20 Customers, Accounts, Bills, and Transactions)
        logger.LogInformation("Starting bulk data generation...");
        var random = new Random(42); // fixed seed for consistency
        var customerRole = roleMap["CUSTOMER"];

        for (int i = 3; i <= 22; i++)
        {
            var email = $"customer{i}@locallink.local";
            var cusCode = $"CUS{i:D6}";
            var accNum = $"10000000{i:D2}";
            
            if (!await context.Users.AnyAsync(u => u.Email == email))
            {
                // Create User
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    Status = UserStatus.Active,
                    CreatedAtUtc = DateTime.UtcNow
                };
                user.PasswordHash = passwordHasher.HashPassword(user, DevDemoPassword);
                user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = customerRole.Id, CreatedAtUtc = DateTime.UtcNow });
                
                // Create Customer
                var customer = new Customer
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    CustomerCode = cusCode,
                    FullName = $"Khach Hang {i}",
                    DateOfBirth = new DateOnly(1990 + random.Next(0, 15), random.Next(1, 13), random.Next(1, 28)),
                    Gender = i % 2 == 0 ? "Male" : "Female",
                    PhoneNumber = $"09{random.Next(10000000, 99999999)}",
                    Address = $"{random.Next(1, 999)} Duong So {random.Next(1, 20)}, TP.HCM",
                    Status = CustomerStatus.Active,
                    CreatedAtUtc = DateTime.UtcNow
                };

                // Create Account
                var account = new BankAccount
                {
                    Id = Guid.NewGuid(),
                    CustomerId = customer.Id,
                    AccountNumber = accNum,
                    AccountName = $"Khach Hang {i} - Checking",
                    AccountType = AccountType.Checking,
                    Balance = random.Next(5, 50) * 1000000m,
                    Currency = "VND",
                    Status = AccountStatus.Active,
                    CreatedAtUtc = DateTime.UtcNow
                };
                
                customer.BankAccounts.Add(account);
                user.Customer = customer;
                context.Users.Add(user);

                // Create Bills
                var providers = new[] { "Da Nang Water", "Da Nang Electricity", "VNPT Internet", "Viettel Post" };
                var types = new[] { BillType.Water, BillType.Electricity, BillType.Internet, BillType.Other };
                for (int b = 0; b < 3; b++)
                {
                    var pIndex = random.Next(providers.Length);
                    context.Bills.Add(new Bill
                    {
                        Id = Guid.NewGuid(),
                        CustomerId = customer.Id,
                        ProviderName = providers[pIndex],
                        BillType = types[pIndex],
                        BillNumber = $"BILL-{2026}-{i:D3}-{b}",
                        Amount = random.Next(5, 50) * 10000m,
                        DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(random.Next(5, 30))),
                        Status = BillStatus.Unpaid,
                        CreatedAtUtc = DateTime.UtcNow
                    });
                }
                
                // Create some fake transactions for today
                var tx = new Transaction
                {
                    Id = Guid.NewGuid(),
                    DestinationAccountId = account.Id,
                    TransactionType = TransactionType.Deposit,
                    Amount = random.Next(1, 10) * 1000000m,
                    ReferenceNumber = $"DEP-{DateTime.UtcNow:yyyyMMdd}-{i}",
                    Description = "Nap tien vao tai khoan",
                    Status = TransactionStatus.Completed,
                    CreatedAtUtc = DateTime.UtcNow,
                    CompletedAtUtc = DateTime.UtcNow
                };
                context.Transactions.Add(tx);
            }
        }
        await context.SaveChangesAsync();

        logger.LogInformation("Database bulk seeding completed successfully.");
    }
}
