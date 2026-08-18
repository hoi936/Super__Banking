using FluentAssertions;
using LocalLink.Domain.Entities;
using LocalLink.Infrastructure.Authentication;
using Xunit;

namespace LocalLink.UnitTests.Authentication;

public class PasswordHasherTests
{
    private readonly PasswordHasherService _hasher = new();

    [Fact]
    public void HashPassword_ShouldReturnNonEmptyHashedString_AndNotMatchPlainText()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "test@locallink.local" };
        var plainPassword = "LocalLink@123";

        // Act
        var hash = _hasher.HashPassword(user, plainPassword);

        // Assert
        hash.Should().NotBeNullOrWhiteSpace();
        hash.Should().NotBe(plainPassword);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnTrue_WhenPasswordIsCorrect()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "test@locallink.local" };
        var plainPassword = "LocalLink@123";
        var hash = _hasher.HashPassword(user, plainPassword);

        // Act
        var isValid = _hasher.VerifyPassword(user, hash, plainPassword);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_WhenPasswordIsIncorrect()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "test@locallink.local" };
        var plainPassword = "LocalLink@123";
        var hash = _hasher.HashPassword(user, plainPassword);

        // Act
        var isValid = _hasher.VerifyPassword(user, hash, "WrongPassword@999");

        // Assert
        isValid.Should().BeFalse();
    }
}
