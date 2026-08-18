using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using LocalLink.Domain.Entities;
using LocalLink.Infrastructure.Authentication;
using Microsoft.Extensions.Options;
using Xunit;

namespace LocalLink.UnitTests.Authentication;

public class TokenServiceTests
{
    private readonly TokenService _tokenService;
    private readonly JwtOptions _options;

    public TokenServiceTests()
    {
        _options = new JwtOptions
        {
            Issuer = "LocalLink",
            Audience = "LocalLink.Client",
            Secret = "SuperSecretDevKeyForUnitTesting_MustBeAtLeast32BytesLong!",
            AccessTokenMinutes = 15,
            RefreshTokenDays = 7
        };

        _tokenService = new TokenService(Options.Create(_options));
    }

    [Fact]
    public void GenerateAccessToken_ShouldProduceValidJwt_WithRequiredClaims()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "customer1@locallink.local"
        };
        var roles = new[] { "CUSTOMER" };
        var customerId = Guid.NewGuid();

        // Act
        var (token, expiresAtUtc) = _tokenService.GenerateAccessToken(user, roles, customerId);

        // Assert
        token.Should().NotBeNullOrWhiteSpace();
        expiresAtUtc.Should().BeAfter(DateTime.UtcNow);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Issuer.Should().Be("LocalLink");
        jwt.Audiences.Should().Contain("LocalLink.Client");
        jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value.Should().Be(user.Id.ToString());
        jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value.Should().Be(user.Email);
        jwt.Claims.First(c => c.Type == "role" || c.Type == ClaimTypes.Role).Value.Should().Be("CUSTOMER");
        jwt.Claims.First(c => c.Type == "customer_id").Value.Should().Be(customerId.ToString());
    }

    [Fact]
    public void GenerateRefreshToken_ShouldProduceCryptographicallySecureToken_AndHash()
    {
        // Act
        var (rawToken, tokenHash, expiresAtUtc) = _tokenService.GenerateRefreshToken("127.0.0.1");

        // Assert
        rawToken.Should().NotBeNullOrWhiteSpace();
        tokenHash.Should().NotBeNullOrWhiteSpace();
        rawToken.Should().NotBe(tokenHash);
        expiresAtUtc.Should().BeAfter(DateTime.UtcNow.AddDays(6));

        // Re-hashing the raw token should produce identical hash
        var computedHash = _tokenService.HashToken(rawToken);
        computedHash.Should().Be(tokenHash);
    }
}
