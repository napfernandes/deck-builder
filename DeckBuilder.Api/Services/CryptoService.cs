using System.Security.Cryptography;
using System.Text;

namespace DeckBuilder.Api.Services;

public static class CryptoService
{
    public static byte[] GenerateSalt(int numberOfBytes = 16)
    {
        var salt = new byte[numberOfBytes];

        using var randomNumberGenerator = RandomNumberGenerator.Create();
        randomNumberGenerator.GetBytes(salt);

        return salt;
    }

    public static byte[] GenerateHash(string stringValue, byte[] saltValue, int hashSize = 20)
    {
        var derivedBytes = new Rfc2898DeriveBytes(stringValue, saltValue, 10000, HashAlgorithmName.SHA512);
        return derivedBytes.GetBytes(hashSize);
    }
}
