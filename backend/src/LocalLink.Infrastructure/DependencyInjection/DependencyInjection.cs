using LocalLink.Application.Accounts.Interfaces;
using LocalLink.Application.Auth.Interfaces;
using LocalLink.Application.Beneficiaries.Interfaces;
using LocalLink.Application.Bills.Interfaces;
using LocalLink.Application.Cards.Interfaces;
using LocalLink.Application.Customers.Interfaces;
using LocalLink.Application.Loans.Interfaces;
using LocalLink.Application.MobileTopUps.Interfaces;
using LocalLink.Application.Napas.Interfaces;
using LocalLink.Application.Notifications.Interfaces;
using LocalLink.Application.Payments.Interfaces;
using LocalLink.Application.PaymentGateway.Interfaces;
using LocalLink.Application.QrPay.Interfaces;
using LocalLink.Application.TermDeposits.Interfaces;
using LocalLink.Application.Transactions.Interfaces;
using LocalLink.Application.Transfers.Interfaces;
using LocalLink.Infrastructure.Authentication;
using LocalLink.Infrastructure.Persistence;
using LocalLink.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LocalLink.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                });
            }
        });

        // JWT Configuration Options
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        // Authentication Services
        services.AddSingleton<IPasswordHasherService, PasswordHasherService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();

        // Customer & Banking Services
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IAdminCustomerService, AdminCustomerService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IAdminAccountService, AdminAccountService>();
        services.AddScoped<IBeneficiaryService, BeneficiaryService>();
        services.AddScoped<ICardService, CardService>();

        // Transfer & Transaction Services
        services.AddScoped<ITransferService, TransferService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<ITermDepositService, TermDepositService>();
        services.AddScoped<INapasTransferService, NapasTransferService>();
        services.AddScoped<ILoanService, LoanService>();
        services.AddScoped<IMobileTopUpService, MobileTopUpService>();
        services.AddScoped<IQrPayService, QrPayService>();
        services.AddScoped<IPaymentGatewayService, PaymentGatewayService>();

        // Bill, Payment & Notification Services
        services.AddScoped<IBillService, BillService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<INotificationService, NotificationService>();

        // Admin Services
        services.AddScoped<LocalLink.Application.Admin.Interfaces.IAdminDashboardService, LocalLink.Infrastructure.Services.Admin.AdminDashboardService>();
        services.AddScoped<LocalLink.Application.Admin.Interfaces.IAdminTransactionService, LocalLink.Infrastructure.Services.Admin.AdminTransactionService>();
        services.AddScoped<LocalLink.Application.Admin.Interfaces.IAdminPaymentService, LocalLink.Infrastructure.Services.Admin.AdminPaymentService>();
        services.AddScoped<LocalLink.Application.Admin.Interfaces.IAdminUserService, LocalLink.Infrastructure.Services.Admin.AdminUserService>();
        services.AddScoped<LocalLink.Application.Admin.Interfaces.IAdminAuditService, LocalLink.Infrastructure.Services.Admin.AdminAuditService>();
        services.AddScoped<IAdminLoanService, LocalLink.Infrastructure.Services.Admin.AdminLoanService>();

        return services;
    }
}
