using SkillNet.Server.Utilities;

namespace SkillNet.Server.Services;

public interface IPasswordHashService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
}

public class PasswordHashService : IPasswordHashService
{
    public string HashPassword(string password) => PasswordHasher.HashPassword(password);

    public bool VerifyPassword(string password, string hashedPassword) => PasswordHasher.VerifyPassword(password, hashedPassword);
}
