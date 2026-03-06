// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Domain.Exceptions;
using ApiKeyGateway.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiKeyGateway.Controllers;

/// <summary>
/// API endpoints for managing API keys
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ApiKeysController : ControllerBase
{
    private readonly IApiKeyService _apiKeyService;
    private readonly IApiKeyRotationService _rotationService;
    private readonly ILogger<ApiKeysController> _logger;

    public ApiKeysController(
        IApiKeyService apiKeyService,
        IApiKeyRotationService rotationService,
        ILogger<ApiKeysController> logger)
    {
        _apiKeyService = apiKeyService ?? throw new ArgumentNullException(nameof(apiKeyService));
        _rotationService = rotationService ?? throw new ArgumentNullException(nameof(rotationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new API key for a consumer
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CreateKeyResponse>> CreateKey([FromBody] CreateKeyRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.ConsumerId) || string.IsNullOrWhiteSpace(request?.Name))
            return BadRequest(new { error = "Consumer ID and Key name are required" });

        try
        {
            var apiKey = await _apiKeyService.CreateKeyAsync(request.ConsumerId, request.Name, request.ExpirationDays);
            _logger.LogInformation("API key created for consumer {ConsumerId}", request.ConsumerId);

            return CreatedAtAction(nameof(GetKeyById), new { id = apiKey.Id }, new CreateKeyResponse
            {
                KeyId = apiKey.Id,
                ConsumerId = apiKey.ConsumerId,
                Name = apiKey.Name,
                ExpiresAt = apiKey.ExpiresAt,
                CreatedAt = apiKey.CreatedAt
            });
        }
        catch (InvalidApiKeyException ex)
        {
            _logger.LogWarning(ex, "Invalid API Key creation attempt: {ErrorMessage}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating API key");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Failed to create API key" });
        }
    }

    /// <summary>
    /// Retrieves an API key by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetKeyResponse>> GetKeyById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest(new { error = "Key ID is required" });

        var apiKey = await _apiKeyService.GetByIdAsync(id);
        if (apiKey == null)
            return NotFound(new { error = "API key not found" });

        return Ok(new GetKeyResponse
        {
            KeyId = apiKey.Id,
            ConsumerId = apiKey.ConsumerId,
            Name = apiKey.Name,
            Status = apiKey.Status.ToString(),
            CreatedAt = apiKey.CreatedAt,
            ExpiresAt = apiKey.ExpiresAt,
            LastUsedAt = apiKey.LastUsedAt,
            RequestCount = apiKey.RequestCount,
            IsActive = apiKey.IsActive
        });
    }

    /// <summary>
    /// Lists all API keys for a consumer
    /// </summary>
    [HttpGet("consumer/{consumerId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<GetKeyResponse>>> GetConsumerKeys(string consumerId)
    {
        if (string.IsNullOrWhiteSpace(consumerId))
            return BadRequest(new { error = "Consumer ID is required" });

        var keys = await _apiKeyService.GetConsumerKeysAsync(consumerId);

        var response = keys.Select(k => new GetKeyResponse
        {
            KeyId = k.Id,
            ConsumerId = k.ConsumerId,
            Name = k.Name,
            Status = k.Status.ToString(),
            CreatedAt = k.CreatedAt,
            ExpiresAt = k.ExpiresAt,
            LastUsedAt = k.LastUsedAt,
            RequestCount = k.RequestCount,
            IsActive = k.IsActive
        }).ToList();

        return Ok(response);
    }

    /// <summary>
    /// Disables an API key
    /// </summary>
    [HttpPut("{id}/disable")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> DisableKey(string id)
    {
        var result = await _apiKeyService.DisableKeyAsync(id);
        if (!result)
            return NotFound(new { error = "API key not found" });

        return Ok(new { message = "API key disabled successfully" });
    }

    /// <summary>
    /// Enables a previously disabled API key
    /// </summary>
    [HttpPut("{id}/enable")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> EnableKey(string id)
    {
        var result = await _apiKeyService.EnableKeyAsync(id);
        if (!result)
            return NotFound(new { error = "API key not found" });

        return Ok(new { message = "API key enabled successfully" });
    }

    /// <summary>
    /// Revokes an API key permanently
    /// </summary>
    [HttpPut("{id}/revoke")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> RevokeKey(string id)
    {
        var result = await _apiKeyService.RevokeKeyAsync(id);
        if (!result)
            return NotFound(new { error = "API key not found" });

        return Ok(new { message = "API key revoked successfully" });
    }

    /// <summary>
    /// Gets the IP whitelist for an API key
    /// </summary>
    [HttpGet("{id}/ip-whitelist")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IpWhitelistResponse>> GetIpWhitelist(string id)
    {
        var key = await _apiKeyService.GetByIdAsync(id);
        if (key == null)
            return NotFound(new { error = "API key not found" });

        var ips = await _apiKeyService.GetIpWhitelistAsync(id);
        return Ok(new IpWhitelistResponse { KeyId = id, AllowedIps = ips, IsUnrestricted = ips.Count == 0 });
    }

    /// <summary>
    /// Replaces the IP whitelist for an API key.
    /// Send an empty list to remove all IP restrictions.
    /// </summary>
    [HttpPut("{id}/ip-whitelist")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IpWhitelistResponse>> SetIpWhitelist(
        string id,
        [FromBody] SetIpWhitelistRequest request)
    {
        if (request is null)
            return BadRequest(new { error = "Request body is required" });

        var success = await _apiKeyService.SetIpWhitelistAsync(id, request.AllowedIps ?? []);
        if (!success)
            return NotFound(new { error = "API key not found" });

        var updated = await _apiKeyService.GetIpWhitelistAsync(id);
        _logger.LogInformation("IP whitelist set for key {KeyId}: {Count} entries", id, updated.Count);

        return Ok(new IpWhitelistResponse { KeyId = id, AllowedIps = updated, IsUnrestricted = updated.Count == 0 });
    }

    /// <summary>
    /// Adds a single IP address to an API key's whitelist
    /// </summary>
    [HttpPost("{id}/ip-whitelist")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<IpWhitelistResponse>> AddIpToWhitelist(
        string id,
        [FromBody] IpAddressRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.IpAddress))
            return BadRequest(new { error = "IP address is required" });

        var key = await _apiKeyService.GetByIdAsync(id);
        if (key == null)
            return NotFound(new { error = "API key not found" });

        var added = await _apiKeyService.AddIpToWhitelistAsync(id, request.IpAddress);
        if (!added)
            return Conflict(new { error = $"IP address '{request.IpAddress}' is already in the whitelist" });

        var updated = await _apiKeyService.GetIpWhitelistAsync(id);
        _logger.LogInformation("IP {Ip} added to whitelist for key {KeyId}", request.IpAddress, id);

        return Ok(new IpWhitelistResponse { KeyId = id, AllowedIps = updated, IsUnrestricted = false });
    }

    /// <summary>
    /// Removes a single IP address from an API key's whitelist
    /// </summary>
    [HttpDelete("{id}/ip-whitelist/{ip}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IpWhitelistResponse>> RemoveIpFromWhitelist(string id, string ip)
    {
        var key = await _apiKeyService.GetByIdAsync(id);
        if (key == null)
            return NotFound(new { error = "API key not found" });

        var removed = await _apiKeyService.RemoveIpFromWhitelistAsync(id, ip);
        if (!removed)
            return NotFound(new { error = $"IP address '{ip}' was not found in the whitelist" });

        var updated = await _apiKeyService.GetIpWhitelistAsync(id);
        _logger.LogInformation("IP {Ip} removed from whitelist for key {KeyId}", ip, id);

        return Ok(new IpWhitelistResponse { KeyId = id, AllowedIps = updated, IsUnrestricted = updated.Count == 0 });
    }

    /// <summary>
    /// Rotates an API key by creating a new key and revoking the old one
    /// </summary>
    [HttpPost("{id}/rotate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RotateKeyResponse>> RotateKey(string id, [FromBody] RotateKeyRequest? request = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest(new { error = "Key ID is required" });

        try
        {
            var result = await _rotationService.RotateKeyAsync(id, request?.NewExpirationDays);

            if (!result.Success)
            {
                if (result.FailureReason == "Key not found")
                    return NotFound(new { error = result.FailureReason });

                return BadRequest(new { error = result.FailureReason });
            }

            _logger.LogInformation(
                "API key {OldKeyId} rotated to {NewKeyId}",
                result.OldKeyId, result.NewKeyId);

            return Ok(new RotateKeyResponse
            {
                OldKeyId = result.OldKeyId,
                NewKeyId = result.NewKeyId,
                ConsumerId = result.ConsumerId,
                NewKeyExpiresAt = result.NewKeyExpiresAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rotating API key {KeyId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Failed to rotate API key" });
        }
    }

    /// <summary>
    /// Deletes an API key
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteKey(string id)
    {
        var key = await _apiKeyService.GetByIdAsync(id);
        if (key == null)
            return NotFound();

        // Revoke instead of delete
        await _apiKeyService.RevokeKeyAsync(id);
        return NoContent();
    }
}

/// <summary>
/// Request model for creating an API key
/// </summary>
public class CreateKeyRequest
{
    public string? ConsumerId { get; set; }
    public string? Name { get; set; }
    public int? ExpirationDays { get; set; }
}

/// <summary>
/// Response model for API key creation
/// </summary>
public class CreateKeyResponse
{
    public string KeyId { get; set; } = string.Empty;
    public string ConsumerId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Response model for retrieving an API key
/// </summary>
public class GetKeyResponse
{
    public string KeyId { get; set; } = string.Empty;
    public string ConsumerId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public int RequestCount { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Request model for rotating an API key
/// </summary>
public class RotateKeyRequest
{
    /// <summary>
    /// Optional expiration in days for the replacement key.
    /// When not provided the original key's TTL is reused.
    /// </summary>
    public int? NewExpirationDays { get; set; }
}

/// <summary>
/// Response model for a key rotation operation
/// </summary>
public class RotateKeyResponse
{
    public string OldKeyId { get; set; } = string.Empty;
    public string NewKeyId { get; set; } = string.Empty;
    public string ConsumerId { get; set; } = string.Empty;
    public DateTime? NewKeyExpiresAt { get; set; }
}

/// <summary>
/// Response model for IP whitelist operations
/// </summary>
public class IpWhitelistResponse
{
    public string KeyId { get; set; } = string.Empty;

    /// <summary>Addresses currently allowed. Empty means all IPs are permitted.</summary>
    public List<string> AllowedIps { get; set; } = [];

    /// <summary>True when no IP restriction is set (all IPs are allowed).</summary>
    public bool IsUnrestricted { get; set; }
}

/// <summary>
/// Request model for replacing the full IP whitelist
/// </summary>
public class SetIpWhitelistRequest
{
    /// <summary>
    /// Complete list of allowed IP addresses.
    /// Pass an empty array to remove all restrictions.
    /// </summary>
    public List<string>? AllowedIps { get; set; }
}

/// <summary>
/// Request model for adding or checking a single IP address
/// </summary>
public class IpAddressRequest
{
    public string? IpAddress { get; set; }
}
