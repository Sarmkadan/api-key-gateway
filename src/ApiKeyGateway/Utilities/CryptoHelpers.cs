// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Security.Cryptography;
using System.Text;

namespace ApiKeyGateway.Utilities;

/// <summary>
/// Cryptography and hashing utility methods
/// </summary>
public static class CryptoHelpers
{
    /// <summary>
    /// Generates a cryptographically secure random string
    /// </summary>
    public static string GenerateSecureRandomString(int length)
    {
        if (length <= 0)
            throw new ArgumentException("Length must be positive", nameof(length));

        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var result = new char[length];

        using var rng = RandomNumberGenerator.Create();
        var buffer = new byte[sizeof(uint)];

        for (int i = 0; i < length; i++)
        {
            rng.GetBytes(buffer);
            uint randomValue = BitConverter.ToUInt32(buffer, 0);
            result[i] = chars[(int)(randomValue % (uint)chars.Length)];
        }

        return new string(result);
    }

    /// <summary>
    /// Computes SHA-256 hash of a string
    /// </summary>
    public static string ComputeSha256Hash(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input cannot be empty", nameof(input));

        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(hashedBytes);
    }

    /// <summary>
    /// Verifies a hash against an input value
    /// </summary>
    public static bool VerifyHash(string input, string hash)
    {
        if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(hash))
            return false;

        var computedHash = ComputeSha256Hash(input);
        return computedHash == hash;
    }

    /// <summary>
    /// Computes HMAC-SHA256 signature
    /// </summary>
    public static string ComputeHmacSha256(string message, string secret)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message cannot be empty", nameof(message));

        if (string.IsNullOrWhiteSpace(secret))
            throw new ArgumentException("Secret cannot be empty", nameof(secret));

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
        return Convert.ToBase64String(signatureBytes);
    }
}
