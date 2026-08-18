using LocalLink.API.Extensions;
using LocalLink.API.Middleware;
using LocalLink.Infrastructure.DependencyInjection;
using LocalLink.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddAppCors(builder.Configuration);
builder.Services.AddAppHealthChecks(builder.Configuration);
builder.Services.AddAppSwagger();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseAppSwagger(app.Environment);

app.UseCors(CorsExtensions.CorsPolicyName);

app.UseAppHealthChecks();

app.UseAuthorization();

app.MapControllers();

// Auto-apply migrations at startup in Container / Development environment if enabled
if (app.Environment.IsDevelopment() || Environment.GetEnvironmentVariable("APPLY_MIGRATIONS_AT_STARTUP") == "true")
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();
    if (dbContext != null)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        try
        {
            logger.LogInformation("Checking database migrations...");
            if (dbContext.Database.IsRelational())
            {
                var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    logger.LogInformation("Applying pending EF Core migrations: {Migrations}", string.Join(", ", pendingMigrations));
                    await dbContext.Database.MigrateAsync();
                    logger.LogInformation("Database migrations applied successfully.");
                }
                else
                {
                    logger.LogInformation("Database is up to date. No pending migrations.");
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not automatically apply migrations on startup (SQL Server might still be starting up).");
        }
    }
}

app.Run();
