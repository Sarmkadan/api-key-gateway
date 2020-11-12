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
    private readonly ILogger<ApiKeysController> _logger;

    public ApiKeysController(IApiKeyService apiKeyService, ILogger<ApiKeysController> logger)
    {
        _apiKeyService = apiKeyService ?? throw new ArgumentNullException(nameof(apiKeyService));
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
