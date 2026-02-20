using System.Security.Cryptography;

namespace UpiFraudApi.Services;

public class PasswordHasher
{
    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(32);
        return Convert.ToBase64String(salt.Concat(hash).ToArray());
    }

    public bool Verify(string password, string hashed)
    {
        var bytes = Convert.FromBase64String(hashed);
        var salt = bytes.Take(16).ToArray();
        var storedHash = bytes.Skip(16).ToArray();
        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
        var computed = pbkdf2.GetBytes(32);
        return storedHash.SequenceEqual(computed);
    }
}
