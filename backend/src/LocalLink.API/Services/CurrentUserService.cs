using System.Security.Claims;
using LocalLink.Application.Auth.Interfaces;

namespace LocalLink.API.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var userIdStr = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue("sub");

            return Guid.TryParse(userIdStr, out var userId) ? userId : null;
        }
    }

    public string? Email =>
        _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email)
        ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue("email");

    public IReadOnlyList<string> Roles
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null) return Array.Empty<string>();

            return user.FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .Concat(user.FindAll("role").Select(c => c.Value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public string? IpAddress
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
            {
                var ip = forwardedFor.FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim();
                if (!string.IsNullOrWhiteSpace(ip)) return ip;
            }

            return context.Connection.RemoteIpAddress?.ToString();
        }
    }
}
