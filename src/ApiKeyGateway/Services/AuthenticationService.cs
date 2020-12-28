// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Domain.Exceptions;
using UnauthorizedAccessException = ApiKeyGateway.Domain.Exceptions.UnauthorizedAccessException;
using ApiKeyGateway.Domain.Models;

namespace ApiKeyGateway.Services;

/// <summary>
/// Handles API key validation and request authentication
/// </summary>
public interface IAuthenticationService
{
    Task<ApiKey> AuthenticateAsync(string apiKey, string? ipAddress = null);
    Task<bool> ValidateIpAsync(ApiKey key, string ipAddress);
    Task LogAuthenticationAttemptAsync(string apiKeyId, bool success, string? reason = null);
}

public class AuthenticationService : IAuthenticationService
{
    private readonly IApiKeyService _apiKeyService;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        IApiKeyService apiKeyService,
        IAuditLogService auditLogService,
        ILogger<AuthenticationService> logger)
    {
        _apiKeyService = apiKeyService ?? throw new ArgumentNullException(nameof(apiKeyService));
        _auditLogService = auditLogService ?? throw new ArgumentNullException(nameof(auditLogService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Authenticates a request using an API key
    /// </summary>
    public async Task<ApiKey> AuthenticateAsync(string apiKey, string? ipAddress = null)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            await LogAuthenticationAttemptAsync("unknown", false, "Missing API key");
            throw new UnauthorizedAccessException(
                Domain.Constants.ErrorMessages.UnauthorizedAccess,
                "Missing API key",
                ipAddress ?? "unknown");
        }

        try
        {
            var validKey = await _apiKeyService.ValidateKeyAsync(apiKey);

            if (validKey == null)
            {
                await LogAuthenticationAttemptAsync("unknown", false, "Invalid API key");
                throw new UnauthorizedAccessException(
                    Domain.Constants.ErrorMessages.ApiKeyNotFound,
                    "Invalid API key format",
                    ipAddress ?? "unknown");
            }

            if (!string.IsNullOrWhiteSpace(ipAddress))
            {
                var isIpAllowed = validKey.IsIpAllowed(ipAddress);
                if (!isIpAllowed)
                {
                    await LogAuthenticationAttemptAsync(validKey.Id, false, $"IP not whitelisted: {ipAddress}");
                    throw new UnauthorizedAccessException(
                        $"Access denied: IP address {ipAddress} is not whitelisted",
                        $"IP address {ipAddress} is not whitelisted",
                        ipAddress);
                }
            }

            validKey.RecordUsage();
            await LogAuthenticationAttemptAsync(validKey.Id, true);

            _logger.LogInformation("API key authenticated successfully: {KeyId} from IP {IpAddress}",
                validKey.Id, ipAddress ?? "unknown");

            return validKey;
        }
        catch (InvalidApiKeyException)
        {
            throw;
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (DataAccessException ex)
        {
            _logger.LogError(ex, "Key store unavailable during authentication for IP {IpAddress}", ipAddress ?? "unknown");
            throw new KeyStoreUnavailableException(
                Domain.Constants.ErrorMessages.KeyStoreUnavailable,
                "ValidateKey",
                ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authentication error");
            throw new UnauthorizedAccessException(
                Domain.Constants.ErrorMessages.UnauthorizedAccess,
                "Authentication service error",
                ipAddress ?? "unknown");
        }
    }

    /// <summary>
    /// Validates if an IP address is allowed for the key
    /// </summary>
    public async Task<bool> ValidateIpAsync(ApiKey key, string ipAddress)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        if (string.IsNullOrWhiteSpace(ipAddress))
            return true;

        var isAllowed = key.IsIpAllowed(ipAddress);

        if (!isAllowed)
        {
            await LogAuthenticationAttemptAsync(key.Id, false, $"IP not whitelisted: {ipAddress}");
        }

        return isAllowed;
    }

    /// <summary>
    /// Logs authentication attempts for audit purposes
    /// </summary>
    public async Task LogAuthenticationAttemptAsync(string apiKeyId, bool success, string? reason = null)
    {
        try
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid().ToString(),
                ResourceId = apiKeyId,
                ResourceType = "ApiKey",
                Action = success ? Domain.Enums.AuditAction.KeyUsed : Domain.Enums.AuditAction.UnauthorizedAttempt,
                IsSuccess = success,
                Reason = reason ?? (success ? "Successful authentication" : "Failed authentication")
            };

            await _auditLogService.LogAsync(auditLog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log authentication attempt");
        }
    }
}
