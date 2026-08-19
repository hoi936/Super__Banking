using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace LocalLink.API.Extensions;

public static class RateLimiterExtensions
{
    public static IServiceCollection AddAppRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: ip,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 100,
                        QueueLimit = 2,
                        Window = TimeSpan.FromMinutes(1)
                    });
            });

            options.AddPolicy("LoginRateLimit", httpContext =>
            {
                var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: ip,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 10,
                        QueueLimit = 0,
                        Window = TimeSpan.FromMinutes(1)
                    });
            });

            options.AddPolicy("FinancialRateLimit", httpContext =>
            {
                var userId = httpContext.User.FindFirst("id")?.Value ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: userId,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 20,
                        QueueLimit = 0,
                        Window = TimeSpan.FromMinutes(1)
                    });
            });

            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    status = 429,
                    title = "Too Many Requests",
                    detail = "You have exceeded your rate limit. Please try again later."
                }, cancellationToken: token);
            };
        });

        return services;
    }
}
