using LocalLink.Application.Auth.Interfaces;
using LocalLink.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace LocalLink.Infrastructure.Authentication;

public class PasswordHasherService : IPasswordHasherService
{
    private readonly PasswordHasher<User> _hasher;

    public PasswordHasherService()
    {
        _hasher = new PasswordHasher<User>();
    }

    public string HashPassword(User user, string password)
    {
        return _hasher.HashPassword(user, password);
    }

    public bool VerifyPassword(User user, string hashedPassword, string providedPassword)
    {
        var result = _hasher.VerifyHashedPassword(user, hashedPassword, providedPassword);
        return result != PasswordVerificationResult.Failed;
    }
}
