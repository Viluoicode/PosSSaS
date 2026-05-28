using PosSSaS.Application.Common.Interfaces;

namespace PosSSaS.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string plain) => BCrypt.Net.BCrypt.HashPassword(plain, workFactor: 11);

    public bool Verify(string plain, string hash)
    {
        try { return BCrypt.Net.BCrypt.Verify(plain, hash); }
        catch { return false; }
    }
}
