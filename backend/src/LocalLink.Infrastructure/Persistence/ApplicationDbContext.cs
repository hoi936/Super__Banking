using System.Reflection;
using Microsoft.EntityFrameworkCore;
using LocalLink.Domain.Entities;

namespace LocalLink.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<SystemInfo> SystemInfos => Set<SystemInfo>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<Beneficiary> Beneficiaries => Set<Beneficiary>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Transfer> Transfers => Set<Transfer>();
    public DbSet<Bill> Bills => Set<Bill>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<TermDeposit> TermDeposits => Set<TermDeposit>();
    public DbSet<ExternalTransfer> ExternalTransfers => Set<ExternalTransfer>();
    public DbSet<BankCard> Cards => Set<BankCard>();
    public DbSet<LoanApplication> LoanApplications => Set<LoanApplication>();
    public DbSet<MobileTopUp> MobileTopUps => Set<MobileTopUp>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<GatewayTransaction> GatewayTransactions => Set<GatewayTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
