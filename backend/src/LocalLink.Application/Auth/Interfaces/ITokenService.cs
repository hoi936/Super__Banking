using LocalLink.Domain.Entities;

namespace LocalLink.Application.Auth.Interfaces;

public interface ITokenService
{
    (string Token, DateTime ExpiresAtUtc) GenerateAccessToken(User user, IEnumerable<string> roles, Guid? customerId = null);
    (string RawToken, string TokenHash, DateTime ExpiresAtUtc) GenerateRefreshToken(string? ipAddress = null);
    string HashToken(string rawToken);
}
