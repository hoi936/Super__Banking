using LocalLink.Application.Accounts.Interfaces;
using LocalLink.Application.Auth.Interfaces;
using LocalLink.Application.Beneficiaries.Interfaces;
using LocalLink.Application.Customers.Interfaces;
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

        return services;
    }
}
