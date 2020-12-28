// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Buffers;
using System.Security.Cryptography;
using System.Text;

namespace ApiKeyGateway.Utilities;

/// <summary>
/// Cryptography and hashing utility methods
/// </summary>
public static class CryptoHelpers
{
    /// <summary>
    /// Generates a cryptographically secure random string.
    /// Delegates to <see cref="RandomNumberGenerator.GetString"/>, which performs
    /// unbiased (rejection-sampled) selection; a plain modulo over the RNG output
    /// would skew the distribution because 2^32 is not divisible by the alphabet size.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="length"/> is not positive.</exception>
    public static string GenerateSecureRandomString(int length)
    {
        if (length <= 0)
            throw new ArgumentException("Length must be positive", nameof(length));

        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return RandomNumberGenerator.GetString(chars, length);
    }

    /// <summary>
    /// Computes SHA-256 hash of a string.
    /// Uses ArrayPool for the UTF-8 encoding buffer and stackalloc for the
    /// 32-byte hash output, keeping GC pressure near zero on the hot path.
    /// </summary>
    public static string ComputeSha256Hash(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input cannot be empty", nameof(input));

        int maxBytes = Encoding.UTF8.GetMaxByteCount(input.Length);
        byte[] rentedBuffer = ArrayPool<byte>.Shared.Rent(maxBytes);
        try
        {
            int byteCount = Encoding.UTF8.GetBytes(input.AsSpan(), rentedBuffer);
            Span<byte> hashBytes = stackalloc byte[SHA256.HashSizeInBytes];
            SHA256.HashData(rentedBuffer.AsSpan(0, byteCount), hashBytes);
            return Convert.ToBase64String(hashBytes);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rentedBuffer);
        }
    }

    /// <summary>
    /// Verifies a hash against an input value using a fixed-time comparison
    /// so the check does not leak how many leading characters matched.
    /// </summary>
    public static bool VerifyHash(string input, string hash)
    {
        if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(hash))
            return false;

        var computedHash = ComputeSha256Hash(input);
        var computedBytes = Encoding.UTF8.GetBytes(computedHash);
        var expectedBytes = Encoding.UTF8.GetBytes(hash);
        return CryptographicOperations.FixedTimeEquals(computedBytes, expectedBytes);
    }

    /// <summary>
    /// Computes HMAC-SHA256 signature.
    /// Uses ArrayPool for both key and message encoding buffers and the static
    /// HMACSHA256.HashData overload to avoid allocating an HMACSHA256 instance.
    /// </summary>
    public static string ComputeHmacSha256(string message, string secret)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message cannot be empty", nameof(message));

        if (string.IsNullOrWhiteSpace(secret))
            throw new ArgumentException("Secret cannot be empty", nameof(secret));

        int maxSecretBytes = Encoding.UTF8.GetMaxByteCount(secret.Length);
        int maxMsgBytes = Encoding.UTF8.GetMaxByteCount(message.Length);

        byte[] secretBuffer = ArrayPool<byte>.Shared.Rent(maxSecretBytes);
        byte[] msgBuffer = ArrayPool<byte>.Shared.Rent(maxMsgBytes);
        try
        {
            int secretByteCount = Encoding.UTF8.GetBytes(secret.AsSpan(), secretBuffer);
            int msgByteCount = Encoding.UTF8.GetBytes(message.AsSpan(), msgBuffer);

            Span<byte> signature = stackalloc byte[HMACSHA256.HashSizeInBytes];
            HMACSHA256.HashData(
                secretBuffer.AsSpan(0, secretByteCount),
                msgBuffer.AsSpan(0, msgByteCount),
                signature);
            return Convert.ToBase64String(signature);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(secretBuffer);
            ArrayPool<byte>.Shared.Return(msgBuffer);
        }
    }
}
