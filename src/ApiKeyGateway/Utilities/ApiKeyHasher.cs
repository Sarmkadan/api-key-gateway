// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Security.Cryptography;
using System.Text;

namespace ApiKeyGateway.Utilities;

/// <summary>
/// Interface for hashing API keys with support for constant-time comparison
/// </summary>
public interface IApiKeyHasher
{
    /// <summary>
    /// Hashes an API key value for secure storage
    /// </summary>
    /// <param name="apiKey">The raw API key value to hash.</param>
    /// <returns>The hashed API key value (base64 encoded).</returns>
    /// <exception cref="ArgumentException">Thrown if apiKey is null or empty.</exception>
    string Hash(string apiKey);

    /// <summary>
    /// Verifies an API key against a stored hash using constant-time comparison
    /// to prevent timing attacks
    /// </summary>
    /// <param name="apiKey">The raw API key value to verify.</param>
    /// <param name="storedHash">The stored hash to compare against.</param>
    /// <returns>True if the API key matches the stored hash; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown if apiKey or storedHash is null or empty.</exception>
    bool Verify(string apiKey, string storedHash);

    /// <summary>
    /// Extracts the hash version prefix from a stored hash (if present)
    /// </summary>
    /// <param name="storedHash">The stored hash with optional version prefix.</param>
    /// <returns>The version prefix (e.g., "v1"), or empty string if no prefix.</returns>
    /// <exception cref="ArgumentException">Thrown if storedHash is null or empty.</exception>
    string GetHashVersion(string storedHash);

    /// <summary>
    /// Creates a hash with a version prefix for future-proofing
    /// </summary>
    /// <param name="apiKey">The raw API key value to hash.</param>
    /// <param name="version">The hash version to use.</param>
    /// <returns>The versioned hashed API key value.</returns>
    /// <exception cref="ArgumentException">Thrown if apiKey or version is null or empty.</exception>
    string HashWithVersion(string apiKey, string version = "v1");
}

/// <summary>
/// Default implementation of <see cref="IApiKeyHasher"/> using SHA-256 with salt
/// </summary>
public class ApiKeyHasher : IApiKeyHasher
{
    private const string DefaultVersion = "v1";
    private const int SaltLength = 16;
    private const int IterationCount = 100000;

    /// <summary>
    /// Hashes an API key using SHA-256 with salt
    /// </summary>
    /// <remarks>
    /// Uses PBKDF2-like approach with 100,000 iterations for secure key derivation.
    /// The resulting hash includes a random salt and version prefix for future-proofing.
    /// </remarks>
    public string Hash(string apiKey)
    {
        ArgumentException.ThrowIfNullOrEmpty(apiKey, nameof(apiKey));

        return HashWithVersion(apiKey, DefaultVersion);
    }

    /// <summary>
    /// Hashes an API key using SHA-256 with salt and optional version prefix
    /// </summary>
    /// <remarks>
    /// Generates a random salt and performs multiple hash iterations for enhanced security.
    /// The format is: {version}${base64Salt}${base64Hash}
    /// </remarks>
    public string HashWithVersion(string apiKey, string version = DefaultVersion)
    {
        ArgumentException.ThrowIfNullOrEmpty(apiKey, nameof(apiKey));
        ArgumentException.ThrowIfNullOrEmpty(version, nameof(version));

        // Generate a random salt
        var salt = RandomNumberGenerator.GetBytes(SaltLength);

        // Hash the key with PBKDF2-like approach (SHA256 iterations)
        using var sha256 = SHA256.Create();
        var keyBytes = Encoding.UTF8.GetBytes(apiKey);

        // Combine salt + key and hash multiple times
        var combined = new byte[salt.Length + keyBytes.Length];
        Buffer.BlockCopy(salt, 0, combined, 0, salt.Length);
        Buffer.BlockCopy(keyBytes, 0, combined, salt.Length, keyBytes.Length);

        byte[] hashBytes = sha256.ComputeHash(combined);

        // Additional iterations for better security
        for (int i = 1; i < IterationCount; i++)
        {
            hashBytes = sha256.ComputeHash(hashBytes);
        }

        // Create versioned hash: v1$salt$hash
        var result = $"{version}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hashBytes)}";
        return result;
    }

    /// <summary>
    /// Verifies an API key against a stored hash using constant-time comparison
    /// to prevent timing attacks
    /// </summary>
    /// <remarks>
    /// Uses <see cref="CryptographicOperations.FixedTimeEquals"/> to prevent timing attacks.
    /// Supports both versioned hashes (v1$...) and legacy format without version.
    /// </remarks>
    public bool Verify(string apiKey, string storedHash)
    {
        ArgumentException.ThrowIfNullOrEmpty(apiKey, nameof(apiKey));
        ArgumentException.ThrowIfNullOrEmpty(storedHash, nameof(storedHash));

        try
        {
            // Extract salt and hash from stored value
            var parts = storedHash.Split('$');
            if (parts.Length != 3)
            {
                // Legacy format without version - hash directly and compare
                var hashedInput = Hash(apiKey);
                return CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(hashedInput),
                    Encoding.UTF8.GetBytes(storedHash)
                );
            }

            var salt = Convert.FromBase64String(parts[1]);
            var expectedHash = Convert.FromBase64String(parts[2]);

            // Hash the input with the same salt
            using var sha256 = SHA256.Create();
            var keyBytes = Encoding.UTF8.GetBytes(apiKey);
            var combined = new byte[salt.Length + keyBytes.Length];
            Buffer.BlockCopy(salt, 0, combined, 0, salt.Length);
            Buffer.BlockCopy(keyBytes, 0, combined, salt.Length, keyBytes.Length);

            byte[] hashBytes = sha256.ComputeHash(combined);

            // Additional iterations for consistency
            for (int i = 1; i < IterationCount; i++)
            {
                hashBytes = sha256.ComputeHash(hashBytes);
            }

            // Use constant-time comparison to prevent timing attacks
            return CryptographicOperations.FixedTimeEquals(hashBytes, expectedHash);
        }
        catch
        {
            // If any error occurs during verification, return false
            // to prevent information leakage
            return false;
        }
    }

    /// <summary>
    /// Extracts the hash version prefix from a stored hash
    /// </summary>
    /// <remarks>
    /// The version is the first component before the first '$' delimiter.
    /// Returns "v1" as default if no version prefix is present.
    /// </remarks>
    public string GetHashVersion(string storedHash)
    {
        ArgumentException.ThrowIfNullOrEmpty(storedHash, nameof(storedHash));

        var parts = storedHash.Split('$');
        return parts.Length >= 1 ? parts[0] : DefaultVersion;
    }
}

/// <summary>
/// Factory for creating <see cref="IApiKeyHasher"/> instances
/// </summary>
public static class ApiKeyHasherFactory
{
    /// <summary>
    /// Creates a new instance of <see cref="ApiKeyHasher"/>
    /// </summary>
    /// <returns>A new <see cref="IApiKeyHasher"/> instance.</returns>
    public static IApiKeyHasher Create() => new ApiKeyHasher();
}