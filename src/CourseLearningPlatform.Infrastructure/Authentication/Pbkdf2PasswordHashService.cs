using System.Security.Cryptography;
using CourseLearningPlatform.Application.Users.Interfaces;

namespace CourseLearningPlatform.Infrastructure.Authentication;

public sealed class Pbkdf2PasswordHashService : IPasswordHashService
{
    private const int Iterations = 310_000;
    private const int SaltSize = 16;
    private const int HashSize = 32;

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
        return $"PBKDF2-SHA256${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public bool Verify(string passwordHash, string password)
    {
        var parts = passwordHash.Split('$');
        if (parts.Length != 4 || parts[0] != "PBKDF2-SHA256" ||
            !int.TryParse(parts[1], out var iterations) || iterations < 100_000 || iterations > 2_000_000)
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(parts[2]);
            var expected = Convert.FromBase64String(parts[3]);
            if (salt.Length != SaltSize || expected.Length != HashSize) return false;
            var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
