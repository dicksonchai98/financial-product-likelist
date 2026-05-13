using System.Security.Cryptography;

namespace FinancialProductLikelist.Services;

public sealed class PasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;

    public (string hash, string salt) Hash(string password)
    {
        var saltBytes = RandomNumberGenerator.GetBytes(SaltSize);
        var keyBytes = Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, Iterations, HashAlgorithmName.SHA256, KeySize);
        return (Convert.ToBase64String(keyBytes), Convert.ToBase64String(saltBytes));
    }

    public bool Verify(string password, string passwordHash, string passwordSalt)
    {
        var saltBytes = Convert.FromBase64String(passwordSalt);
        var keyBytes = Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, Iterations, HashAlgorithmName.SHA256, KeySize);
        var currentHash = Convert.ToBase64String(keyBytes);
        return CryptographicOperations.FixedTimeEquals(
            Convert.FromBase64String(currentHash),
            Convert.FromBase64String(passwordHash));
    }
}
