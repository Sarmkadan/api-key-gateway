// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Domain.Exceptions;
using ApiKeyGateway.Domain.Models;
using System.Security.Cryptography;
using System.Text;
using System.Runtime.CompilerServices;

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
    public async Task<ApiKey> CreateKeyAsync(string consumerId, string name, int? expirationDays = null)
    {
        if (string.IsNullOrWhiteSpace(consumerId))
            throw new ArgumentException("Consumer ID cannot be empty", nameof(consumerId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Key name cannot be empty", nameof(name));

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
        if (string.IsNullOrWhiteSpace(keyId))
            return null;

        return await _repository.GetByIdAsync(keyId);
    }

    /// <summary>
    /// Validates an API key and returns it if valid
    /// </summary>
    public async Task<ApiKey?> ValidateKeyAsync(string keyValue)
    {
        if (string.IsNullOrWhiteSpace(keyValue))
            throw new UnauthorizedAccessException(Domain.Constants.ErrorMessages.UnauthorizedAccess);

        var keyHash = HashApiKey(keyValue);
        var apiKey = await _repository.GetByHashAsync(keyHash);

        if (apiKey == null)
            throw new InvalidApiKeyException(Domain.Constants.ErrorMessages.ApiKeyNotFound, keyHash);

        if (apiKey.IsExpired)
            throw new InvalidApiKeyException(Domain.Constants.ErrorMessages.ApiKeyExpired, isExpired: true);

        if (!apiKey.CanBeUsed())
            throw new InvalidApiKeyException(Domain.Constants.ErrorMessages.ApiKeyDisabled);

        return apiKey;
    }

    /// <summary>
    /// Disables an API key
    /// </summary>
    public async Task<bool> DisableKeyAsync(string keyId)
    {
        var key = await GetByIdAsync(keyId);
        if (key == null)
            return false;

        key.Disable();
        await _repository.UpdateAsync(key);
        _logger.LogInformation("API key {KeyId} disabled", keyId);

        return true;
    }

    /// <summary>
    /// Enables a previously disabled API key
    /// </summary>
    public async Task<bool> EnableKeyAsync(string keyId)
    {
        var key = await GetByIdAsync(keyId);
        if (key == null)
            return false;

        key.Enable();
        await _repository.UpdateAsync(key);
        _logger.LogInformation("API key {KeyId} enabled", keyId);

        return true;
    }

    /// <summary>
    /// Permanently revokes an API key
    /// </summary>
    public async Task<bool> RevokeKeyAsync(string keyId)
    {
        var key = await GetByIdAsync(keyId);
        if (key == null)
            return false;

        key.Revoke();
        await _repository.UpdateAsync(key);
        _logger.LogWarning("API key {KeyId} revoked", keyId);

        return true;
    }

    /// <summary>
    /// Retrieves all API keys for a consumer
    /// </summary>
    public async Task<List<ApiKey>> GetConsumerKeysAsync(string consumerId)
    {
        if (string.IsNullOrWhiteSpace(consumerId))
            return [];

        return await _repository.GetByConsumerIdAsync(consumerId);
    }

    /// <summary>
    /// Updates metadata for an API key
    /// </summary>
    public async Task<bool> UpdateKeyMetadataAsync(string keyId, Dictionary<string, string> metadata)
    {
        var key = await GetByIdAsync(keyId);
        if (key == null)
            return false;

        if (metadata != null)
        {
            foreach (var kvp in metadata)
            {
                key.Metadata[kvp.Key] = kvp.Value;
            }
        }

        await _repository.UpdateAsync(key);
        return true;
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
}
