using LocalLink.Domain.Entities;

namespace LocalLink.Application.Auth.Interfaces;

public interface IPasswordHasherService
{
    string HashPassword(User user, string password);
    bool VerifyPassword(User user, string hashedPassword, string providedPassword);
}
