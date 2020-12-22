// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Domain.Exceptions;
using ApiKeyGateway.Domain.Models;

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
    /// Expiration for the new key in days. When <c>null</c> the same TTL as the
    /// original key is used; if the original had no expiry the new key also has none.
    /// </param>
    Task<RotationResult> RotateKeyAsync(string keyId, int? newExpirationDays = null);

    /// <summary>
    /// Rotates all active keys whose expiry falls within <paramref name="warningDays"/> from now.
    /// </summary>
    /// <param name="warningDays">Look-ahead window in days (default 7).</param>
    /// <param name="newExpirationDays">
    /// Expiration for the replacement keys. Defaults to the same TTL as each original.
    /// </param>
    Task<List<RotationResult>> RotateExpiringSoonAsync(int warningDays = 7, int? newExpirationDays = null);
}

/// <summary>
/// Default implementation of <see cref="IApiKeyRotationService"/>.
/// </summary>
public class ApiKeyRotationService : IApiKeyRotationService
{
    private readonly IApiKeyService _apiKeyService;
    private readonly IApiKeyRepository _repository;
    private readonly ILogger<ApiKeyRotationService> _logger;

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

        var oldKey = await _repository.GetByIdAsync(keyId);
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

        try
        {
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

            _logger.LogInformation(
                "Rotated API key {OldKeyId} → {NewKeyId} for consumer {ConsumerId}",
                keyId, newKey.Id, oldKey.ConsumerId);

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
            _logger.LogError(ex, "Failed to rotate API key {KeyId}", keyId);
            return new RotationResult
            {
                OldKeyId = keyId,
                ConsumerId = oldKey.ConsumerId,
                Success = false,
                FailureReason = ex.Message
            };
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

        _logger.LogInformation(
            "Found {Count} API keys expiring within {Days} days",
            expiringKeys.Count, warningDays);

        var results = new List<RotationResult>(expiringKeys.Count);
        foreach (var key in expiringKeys)
        {
            var result = await RotateKeyAsync(key.Id, newExpirationDays);
            results.Add(result);
        }

        var succeeded = results.Count(r => r.Success);
        _logger.LogInformation(
            "Key rotation complete: {Succeeded}/{Total} keys rotated successfully",
            succeeded, results.Count);

        return results;
    }
}
