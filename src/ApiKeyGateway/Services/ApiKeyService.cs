// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Domain.Exceptions;
using ApiKeyGateway.Domain.Models;
using UnauthorizedAccessException = ApiKeyGateway.Domain.Exceptions.UnauthorizedAccessException;
using System.Security.Cryptography;
using System.Text;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging; // Ensure logging namespace is available

namespace ApiKeyGateway.Services;

/// <summary>
/// Manages API key lifecycle - creation, validation, revocation, and queries
/// </summary>
public interface IApiKeyService
{
    Task<ApiKey> CreateKeyAsync(string consumerId, string name, int? expirationDays = null);
    Task<ApiKey?> GetByIdAsync(string keyId);
    Task<ApiKey?> ValidateKeyAsync(string keyValue);
    Task<bool> DisableKeyAsync(string keyId);
    Task<bool> EnableKeyAsync(string keyId);
    Task<bool> RevokeKeyAsync(string keyId);
    Task<List<ApiKey>> GetConsumerKeysAsync(string consumerId);
    Task<bool> UpdateKeyMetadataAsync(string keyId, Dictionary<string, string> metadata);

    // IP whitelist management
    Task<List<string>> GetIpWhitelistAsync(string keyId);
    Task<bool> SetIpWhitelistAsync(string keyId, IEnumerable<string> ips);
    Task<bool> AddIpToWhitelistAsync(string keyId, string ip);
    Task<bool> RemoveIpFromWhitelistAsync(string keyId, string ip);
}

public class ApiKeyService : IApiKeyService
{
    private const string KeyPrefix = "sk";
    private const int RandomPartLength = 32;
    private readonly IApiKeyRepository _repository;
    private readonly ILogger<ApiKeyService> _logger;

    public ApiKeyService(IApiKeyRepository repository, ILogger<ApiKeyService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new API key for a consumer with optional expiration
    /// </summary>
    /// <param name="consumerId">The ID of the consumer.</param>
    /// <param name="name">The name of the key.</param>
    /// <param name="expirationDays">Optional expiration days.</param>
    /// <returns>The created <see cref="ApiKey"/>.</returns>
    public async Task<ApiKey> CreateKeyAsync(string consumerId, string name, int? expirationDays = null)
    {
        _logger.LogDebug("Creating API key for consumer {ConsumerId} with name {Name}", consumerId, name);

        if (string.IsNullOrWhiteSpace(consumerId))
            throw new ValidationException("Consumer ID cannot be empty", nameof(consumerId), consumerId);

        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Key name cannot be empty", nameof(name), name);

        try
        {
            var randomPart = GenerateRandomString(RandomPartLength);
            var keyValue = $"{KeyPrefix}_{randomPart}";
            var keyHash = HashApiKey(keyValue);

            var apiKey = new ApiKey
            {
                Id = Guid.NewGuid().ToString(),
                ConsumerId = consumerId,
                Name = name,
                KeyHash = keyHash,
                Prefix = KeyPrefix,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expirationDays.HasValue ? DateTime.UtcNow.AddDays(expirationDays.Value) : null
            };

            var created = await _repository.CreateAsync(apiKey);
            _logger.LogInformation("API key created for consumer {ConsumerId} with name {Name}", consumerId, name);

            return created;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create API key for consumer {ConsumerId}. Exception Type: {ExceptionType}, Message: {ExceptionMessage}", consumerId, ex.GetType().Name, ex.Message);
            throw new InvalidApiKeyException(Domain.Constants.ErrorMessages.KeyGenerationFailed, ex);
        }
    }

    /// <summary>
    /// Retrieves an API key by its ID
    /// </summary>
    public async Task<ApiKey?> GetByIdAsync(string keyId)
    {
        _logger.LogDebug("Retrieving API key by ID {KeyId}", keyId);

        if (string.IsNullOrWhiteSpace(keyId))
            throw new ValidationException("Key ID cannot be empty", nameof(keyId), keyId);

        return await _repository.GetByIdAsync(keyId);
    }

    /// <summary>
    /// Validates an API key and returns it if valid
    /// </summary>
    public async Task<ApiKey?> ValidateKeyAsync(string keyValue)
    {
        _logger.LogDebug("Validating API key value");

        if (string.IsNullOrWhiteSpace(keyValue))
            throw new UnauthorizedAccessException(Domain.Constants.ErrorMessages.UnauthorizedAccess);

        var keyHash = HashApiKey(keyValue);
        var apiKey = await _repository.GetByHashAsync(keyHash);

        if (apiKey == null)
        {
            _logger.LogWarning("API key not found for hash {KeyHash}", keyHash);
            throw new InvalidApiKeyException(Domain.Constants.ErrorMessages.ApiKeyNotFound, keyHash);
        }

        if (apiKey.IsExpired)
        {
            _logger.LogWarning("API key {KeyId} is expired", apiKey.Id);
            throw new InvalidApiKeyException(Domain.Constants.ErrorMessages.ApiKeyExpired, isExpired: true);
        }

        if (!apiKey.CanBeUsed())
        {
            _logger.LogWarning("API key {KeyId} is disabled", apiKey.Id);
            throw new InvalidApiKeyException(Domain.Constants.ErrorMessages.ApiKeyDisabled);
        }

        _logger.LogInformation("API key {KeyId} validated successfully", apiKey.Id);
        return apiKey;
    }

    /// <summary>
    /// Disables an API key
    /// </summary>
    public async Task<bool> DisableKeyAsync(string keyId)
    {
        _logger.LogDebug("Disabling API key {KeyId}", keyId);

        if (string.IsNullOrWhiteSpace(keyId))
            throw new ValidationException("Key ID cannot be empty", nameof(keyId), keyId);

        try
        {
            var key = await GetByIdAsync(keyId);
            if (key == null)
            {
                _logger.LogWarning("Attempted to disable non‑existent key {KeyId}", keyId);
                return false;
            }

            key.Disable();
            await _repository.UpdateAsync(key);
            _logger.LogInformation("API key {KeyId} disabled", keyId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to disable API key {KeyId}", keyId);
            throw new DataAccessException(Domain.Constants.ErrorMessages.KeyUpdateFailed, nameof(DisableKeyAsync), nameof(ApiKey), ex);
        }
    }

    /// <summary>
    /// Enables a previously disabled API key
    /// </summary>
    public async Task<bool> EnableKeyAsync(string keyId)
    {
        _logger.LogDebug("Enabling API key {KeyId}", keyId);

        if (string.IsNullOrWhiteSpace(keyId))
            throw new ValidationException("Key ID cannot be empty", nameof(keyId), keyId);

        try
        {
            var key = await GetByIdAsync(keyId);
            if (key == null)
            {
                _logger.LogWarning("Attempted to enable non‑existent key {KeyId}", keyId);
                return false;
            }

            key.Enable();
            await _repository.UpdateAsync(key);
            _logger.LogInformation("API key {KeyId} enabled", keyId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enable API key {KeyId}", keyId);
            throw new DataAccessException(Domain.Constants.ErrorMessages.KeyUpdateFailed, nameof(EnableKeyAsync), nameof(ApiKey), ex);
        }
    }

    /// <summary>
    /// Permanently revokes an API key
    /// </summary>
    public async Task<bool> RevokeKeyAsync(string keyId)
    {
        _logger.LogDebug("Revoking API key {KeyId}", keyId);

        if (string.IsNullOrWhiteSpace(keyId))
            throw new ValidationException("Key ID cannot be empty", nameof(keyId), keyId);

        try
        {
            var key = await GetByIdAsync(keyId);
            if (key == null)
            {
                _logger.LogWarning("Attempted to revoke non‑existent key {KeyId}", keyId);
                return false;
            }

            key.Revoke();
            await _repository.UpdateAsync(key);
            _logger.LogWarning("API key {KeyId} revoked", keyId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to revoke API key {KeyId}", keyId);
            throw new DataAccessException(Domain.Constants.ErrorMessages.KeyUpdateFailed, nameof(RevokeKeyAsync), nameof(ApiKey), ex);
        }
    }

    /// <summary>
    /// Retrieves all API keys for a consumer
    /// </summary>
    public async Task<List<ApiKey>> GetConsumerKeysAsync(string consumerId)
    {
        _logger.LogDebug("Getting API keys for consumer {ConsumerId}", consumerId);

        if (string.IsNullOrWhiteSpace(consumerId))
            return [];

        try
        {
            var result = await _repository.GetByConsumerIdAsync(consumerId);
            _logger.LogInformation("Retrieved {Count} keys for consumer {ConsumerId}", result.Count, consumerId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get keys for consumer {ConsumerId}", consumerId);
            throw new DataAccessException(Domain.Constants.ErrorMessages.DataAccessFailed, nameof(GetConsumerKeysAsync), nameof(ApiKey), ex);
        }
    }

    /// <summary>
    /// Updates metadata for an API key
    /// </summary>
    public async Task<bool> UpdateKeyMetadataAsync(string keyId, Dictionary<string, string> metadata)
    {
        _logger.LogDebug("Updating metadata for key {KeyId}", keyId);

        if (string.IsNullOrWhiteSpace(keyId))
            throw new ValidationException("Key ID cannot be empty", nameof(keyId), keyId);

        if (metadata == null)
            throw new ValidationException("Metadata cannot be null", nameof(metadata), metadata);

        try
        {
            var key = await GetByIdAsync(keyId);
            if (key == null)
            {
                _logger.LogWarning("Attempted to update metadata for non‑existent key {KeyId}", keyId);
                return false;
            }

            foreach (var kvp in metadata)
            {
                key.Metadata[kvp.Key] = kvp.Value;
            }

            await _repository.UpdateAsync(key);
            _logger.LogInformation("Metadata updated for key {KeyId}", keyId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update metadata for key {KeyId}", keyId);
            throw new DataAccessException(Domain.Constants.ErrorMessages.KeyUpdateFailed, nameof(UpdateKeyMetadataAsync), nameof(ApiKey), ex);
        }
    }

    /// <summary>
    /// Returns the IP whitelist for a key as a list of individual addresses.
    /// An empty list means the key has no IP restriction.
    /// </summary>
    public async Task<List<string>> GetIpWhitelistAsync(string keyId)
    {
        _logger.LogDebug("Getting IP whitelist for key {KeyId}", keyId);

        if (string.IsNullOrWhiteSpace(keyId))
            throw new ValidationException("Key ID cannot be empty", nameof(keyId), keyId);

        try
        {
            var key = await GetByIdAsync(keyId);
            if (key == null)
            {
                _logger.LogWarning("IP whitelist requested for non‑existent key {KeyId}", keyId);
                return [];
            }

            if (string.IsNullOrWhiteSpace(key.IpWhitelist))
                return [];

            var list = key.IpWhitelist
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(ip => ip.Trim())
                .Where(ip => !string.IsNullOrWhiteSpace(ip))
                .Distinct()
                .ToList();

            _logger.LogInformation("IP whitelist for key {KeyId} contains {Count} entries", keyId, list.Count);
            return list;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get IP whitelist for key {KeyId}", keyId);
            throw new DataAccessException(Domain.Constants.ErrorMessages.DataAccessFailed, nameof(GetIpWhitelistAsync), nameof(ApiKey), ex);
        }
    }

    /// <summary>
    /// Replaces the IP whitelist for a key with the provided set of IPs.
    /// Pass an empty collection to remove all IP restrictions.
    /// </summary>
    public async Task<bool> SetIpWhitelistAsync(string keyId, IEnumerable<string> ips)
    {
        _logger.LogDebug("Setting IP whitelist for key {KeyId}", keyId);

        if (string.IsNullOrWhiteSpace(keyId))
            throw new ValidationException("Key ID cannot be empty", nameof(keyId), keyId);

        try
        {
            var key = await GetByIdAsync(keyId);
            if (key == null)
            {
                _logger.LogWarning("Attempted to set IP whitelist for non‑existent key {KeyId}", keyId);
                return false;
            }

            var validated = ValidateIps(ips);
            key.IpWhitelist = validated.Count > 0 ? string.Join(",", validated) : null;

            await _repository.UpdateAsync(key);
            _logger.LogInformation(
                "IP whitelist updated for key {KeyId}: {Count} addresses",
                keyId, validated.Count);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set IP whitelist for key {KeyId}", keyId);
            throw new DataAccessException(Domain.Constants.ErrorMessages.KeyUpdateFailed, nameof(SetIpWhitelistAsync), nameof(ApiKey), ex);
        }
    }

    /// <summary>
    /// Adds a single IP address to a key's whitelist.
    /// Returns <c>false</c> when the key does not exist or the IP is already present.
    /// </summary>
    public async Task<bool> AddIpToWhitelistAsync(string keyId, string ip)
    {
        _logger.LogDebug("Adding IP {Ip} to whitelist for key {KeyId}", ip, keyId);

        if (string.IsNullOrWhiteSpace(keyId))
            throw new ValidationException("Key ID cannot be empty", nameof(keyId), keyId);

        if (string.IsNullOrWhiteSpace(ip))
            throw new ValidationException("IP address cannot be empty", nameof(ip), ip);

        var current = await GetIpWhitelistAsync(keyId);
        var normalised = ip.Trim();

        if (current.Contains(normalised))
        {
            _logger.LogWarning("IP {Ip} already present in whitelist for key {KeyId}", ip, keyId);
            return false;
        }

        current.Add(normalised);
        var result = await SetIpWhitelistAsync(keyId, current);
        _logger.LogInformation("IP {Ip} added to whitelist for key {KeyId}", ip, keyId);
        return result;
    }

    /// <summary>
    /// Removes a single IP address from a key's whitelist.
    /// Returns <c>false</c> when the key does not exist or the IP was not in the list.
    /// </summary>
    public async Task<bool> RemoveIpFromWhitelistAsync(string keyId, string ip)
    {
        _logger.LogDebug("Removing IP {Ip} from whitelist for key {KeyId}", ip, keyId);

        if (string.IsNullOrWhiteSpace(keyId))
            throw new ValidationException("Key ID cannot be empty", nameof(keyId), keyId);

        if (string.IsNullOrWhiteSpace(ip))
            throw new ValidationException("IP address cannot be empty", nameof(ip), ip);

        var current = await GetIpWhitelistAsync(keyId);
        var normalised = ip.Trim();

        if (!current.Remove(normalised))
        {
            _logger.LogWarning("IP {Ip} not found in whitelist for key {KeyId}", ip, keyId);
            return false;
        }

        var result = await SetIpWhitelistAsync(keyId, current);
        _logger.LogInformation("IP {Ip} removed from whitelist for key {KeyId}", ip, keyId);
        return result;
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static List<string> ValidateIps(IEnumerable<string> ips)
    {
        return ips
            .Select(ip => ip?.Trim() ?? string.Empty)
            .Where(ip => !string.IsNullOrWhiteSpace(ip))
            .Distinct()
            .ToList();
    }

    /// <summary>
    /// Hashes an API key using SHA-256
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string HashApiKey(string keyValue)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(keyValue));
        return Convert.ToBase64String(hashedBytes);
    }

    /// <summary>
    /// Generates a random string of specified length
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string(Enumerable.Range(0, length).Select(_ => chars[random.Next(chars.Length)]).ToArray());
    }
}

/// <summary>
/// Repository interface for API key data access
/// </summary>
public interface IApiKeyRepository
{
    Task<ApiKey> CreateAsync(ApiKey apiKey);
    Task<ApiKey?> GetByIdAsync(string id);
    Task<ApiKey?> GetByHashAsync(string keyHash);
    Task<List<ApiKey>> GetByConsumerIdAsync(string consumerId);
    Task UpdateAsync(ApiKey apiKey);
    Task DeleteAsync(string id);
    Task<List<ApiKey>> GetExpiredKeysAsync();
    Task<List<ApiKey>> GetAllAsync();
    Task<List<ApiKey>> GetKeysExpiringBeforeAsync(DateTime threshold);
}
