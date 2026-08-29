// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Domain.Exceptions;
using ApiKeyGateway.Domain.Models;
using System.Collections.Concurrent;

namespace ApiKeyGateway.Services;

/// <summary>
/// Result of a single key rotation operation
/// </summary>
public class RotationResult
{
    /// <summary>The ID of the original (old) key that was rotated</summary>
    public string OldKeyId { get; init; } = string.Empty;

    /// <summary>The ID of the newly created replacement key</summary>
    public string NewKeyId { get; init; } = string.Empty;

    /// <summary>Consumer who owns both keys</summary>
    public string ConsumerId { get; init; } = string.Empty;

    /// <summary>Whether the rotation succeeded</summary>
    public bool Success { get; init; }

    /// <summary>Reason for failure, if any</summary>
    public string? FailureReason { get; init; }

    /// <summary>When the new key expires</summary>
    public DateTime? NewKeyExpiresAt { get; init; }
}

/// <summary>
/// Manages automated and manual API key rotation.
/// Rotation creates a new key that inherits the old key's metadata and
/// then revokes the old key so callers must upgrade to the new value.
/// </summary>
public interface IApiKeyRotationService
{
    /// <summary>
    /// Rotates a single key by ID, creating a replacement and revoking the original.
    /// </summary>
    /// <param name="keyId">The ID of the key to rotate.</param>
    /// <param name="newExpirationDays">
    /// Expiration for the new key in days. When <paramref name="newExpirationDays"/> is <c>null</c> the same TTL as the
    /// original key is used; if the original had no expiry the new key also has none.
    /// </param>
    /// <returns>Result of the rotation operation.</returns>
    Task<RotationResult> RotateKeyAsync(string keyId, int? newExpirationDays = null);

    /// <summary>
    /// Rotates all active keys whose expiry falls within <paramref name="warningDays"/> from now.
    /// Useful for proactive rotation of keys approaching expiration.
    /// </summary>
    /// <param name="warningDays">Look-ahead window in days (default 7).</param>
    /// <param name="newExpirationDays">
    /// Expiration for the replacement keys. Defaults to the same TTL as each original.
    /// </param>
    /// <returns>List of rotation results for all keys processed.</returns>
    Task<List<RotationResult>> RotateExpiringSoonAsync(int warningDays = 7, int? newExpirationDays = null);
}

/// <summary>
/// Default implementation of <see cref="IApiKeyRotationService"/>.
/// </summary>
public partial class ApiKeyRotationService : IApiKeyRotationService
{
    private static partial class Log
    {
        [LoggerMessage(
            EventId = 4100,
            Level = LogLevel.Information,
            Message = "Rotated API key {OldKeyId} → {NewKeyId} for consumer {ConsumerId}")]
        public static partial void KeyRotated(
            ILogger logger,
            string oldKeyId,
            string newKeyId,
            string consumerId);

        [LoggerMessage(
            EventId = 4101,
            Level = LogLevel.Error,
            Message = "Failed to rotate API key {ApiKeyId}")]
        public static partial void KeyRotationFailed(
            ILogger logger,
            Exception exception,
            string apiKeyId);

        [LoggerMessage(
            EventId = 4102,
            Level = LogLevel.Information,
            Message = "Found {Count} API keys expiring within {Days} days")]
        public static partial void ExpiringKeysFound(
            ILogger logger,
            int count,
            int days);

        [LoggerMessage(
            EventId = 4103,
            Level = LogLevel.Information,
            Message = "Key rotation complete: {Succeeded}/{Total} keys rotated successfully")]
        public static partial void RotationCompleted(
            ILogger logger,
            int succeeded,
            int total);
    }

    private readonly IApiKeyService _apiKeyService;
    private readonly IApiKeyRepository _repository;
    private readonly ILogger<ApiKeyRotationService> _logger;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _rotationLocks = new();

    public ApiKeyRotationService(
        IApiKeyService apiKeyService,
        IApiKeyRepository repository,
        ILogger<ApiKeyRotationService> logger)
    {
        _apiKeyService = apiKeyService ?? throw new ArgumentNullException(nameof(apiKeyService));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<RotationResult> RotateKeyAsync(string keyId, int? newExpirationDays = null)
    {
        if (string.IsNullOrWhiteSpace(keyId))
            throw new ValidationException("Key ID cannot be empty", nameof(keyId), keyId);

        if (newExpirationDays.HasValue && newExpirationDays <= 0)
            throw new ValidationException("Expiration days must be positive", nameof(newExpirationDays), newExpirationDays);

        // Acquire a per-key lock to prevent concurrent rotations of the same key
        var keyLock = _rotationLocks.GetOrAdd(keyId, _ => new SemaphoreSlim(1, 1));
        await keyLock.WaitAsync();

        ApiKey? oldKey = null;
        try
        {
            oldKey = await _repository.GetByIdAsync(keyId);
            if (oldKey is null)
            {
                return new RotationResult
                {
                    OldKeyId = keyId,
                    Success = false,
                    FailureReason = "Key not found"
                };
            }

            if (!oldKey.IsActive)
            {
                return new RotationResult
                {
                    OldKeyId = keyId,
                    ConsumerId = oldKey.ConsumerId,
                    Success = false,
                    FailureReason = $"Key is not active (status: {oldKey.Status})"
                };
            }

            // Determine expiration for the new key
            int? expirationDays = newExpirationDays;
            if (expirationDays is null && oldKey.ExpiresAt.HasValue)
            {
                var remaining = (oldKey.ExpiresAt.Value - DateTime.UtcNow).TotalDays;
                // Give the same TTL the original key had from its creation date
                var originalTtl = (oldKey.ExpiresAt.Value - oldKey.CreatedAt).TotalDays;
                expirationDays = (int)Math.Ceiling(originalTtl);
            }

            var newKey = await _apiKeyService.CreateKeyAsync(
                oldKey.ConsumerId,
                $"{oldKey.Name} (rotated {DateTime.UtcNow:yyyy-MM-dd})",
                expirationDays);

            // Carry over metadata, IP whitelist and scope restrictions
            newKey.IpWhitelist = oldKey.IpWhitelist;
            newKey.AllowedScopes = oldKey.AllowedScopes;
            if (oldKey.Metadata.Count > 0)
                await _apiKeyService.UpdateKeyMetadataAsync(newKey.Id, oldKey.Metadata);

            // Persist whitelist/scope on the new key
            await _repository.UpdateAsync(newKey);

            // Revoke the old key
            await _apiKeyService.RevokeKeyAsync(keyId);

            Log.KeyRotated(_logger, keyId, newKey.Id, oldKey.ConsumerId);

            return new RotationResult
            {
                OldKeyId = keyId,
                NewKeyId = newKey.Id,
                ConsumerId = oldKey.ConsumerId,
                Success = true,
                NewKeyExpiresAt = newKey.ExpiresAt
            };
        }
        catch (Exception ex)
        {
            Log.KeyRotationFailed(_logger, ex, keyId);
            return new RotationResult
            {
                OldKeyId = keyId,
                ConsumerId = oldKey?.ConsumerId ?? string.Empty,
                Success = false,
                FailureReason = ex.Message
            };
        }
        finally
        {
            keyLock.Release();
            // Clean up the lock if no longer needed
            if (keyLock.CurrentCount == 1)
            {
                _rotationLocks.TryRemove(keyId, out _);
            }
        }
    }

    /// <inheritdoc/>
    public async Task<List<RotationResult>> RotateExpiringSoonAsync(
        int warningDays = 7,
        int? newExpirationDays = null)
    {
        if (warningDays <= 0)
            throw new ValidationException("Warning days must be positive", nameof(warningDays), warningDays);

        if (newExpirationDays.HasValue && newExpirationDays <= 0)
            throw new ValidationException("Expiration days must be positive", nameof(newExpirationDays), newExpirationDays);

        var threshold = DateTime.UtcNow.AddDays(warningDays);
        var expiringKeys = await _repository.GetKeysExpiringBeforeAsync(threshold);

        Log.ExpiringKeysFound(_logger, expiringKeys.Count, warningDays);

        var results = new List<RotationResult>(expiringKeys.Count);
        foreach (var key in expiringKeys)
        {
            var result = await RotateKeyAsync(key.Id, newExpirationDays);
            results.Add(result);
        }

        var succeeded = results.Count(r => r.Success);
        Log.RotationCompleted(_logger, succeeded, results.Count);

        return results;
    }
}
