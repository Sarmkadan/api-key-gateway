// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiKeyGateway.Controllers;

/// <summary>
/// API endpoints for retrieving usage statistics and tracking
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsageController : ControllerBase
{
    private readonly IUsageTrackingService _usageService;
    private readonly ILogger<UsageController> _logger;

    public UsageController(IUsageTrackingService usageService, ILogger<UsageController> logger)
    {
        _usageService = usageService ?? throw new ArgumentNullException(nameof(usageService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves usage statistics for an API key
    /// </summary>
    [HttpGet("keys/{apiKeyId}/statistics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UsageStatisticsResponse>> GetKeyStatistics(
        string apiKeyId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        if (string.IsNullOrWhiteSpace(apiKeyId))
            return BadRequest(new { error = "API Key ID is required" });

        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        if (end < start)
            return BadRequest(new { error = "End date must be after start date" });

        try
        {
            var stats = await _usageService.GetUsageStatisticsAsync(apiKeyId, start, end);
            _logger.LogInformation("Retrieved usage statistics for API key {ApiKeyId}", apiKeyId);

            return Ok(new UsageStatisticsResponse
            {
                ApiKeyId = stats.ApiKeyId,
                StartDate = stats.StartDate,
                EndDate = stats.EndDate,
                TotalRequests = stats.TotalRequests,
                SuccessfulRequests = stats.SuccessfulRequests,
                FailedRequests = stats.FailedRequests,
                SuccessRate = stats.SuccessRate,
                TotalBytesTransferred = stats.TotalBytesTransferred,
                AverageResponseTimeMs = stats.AverageResponseTimeMs,
                UniqueEndpoints = stats.UniqueEndpoints
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving usage statistics");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to retrieve usage statistics" });
        }
    }

    /// <summary>
    /// Retrieves detailed usage records for an API key
    /// </summary>
    [HttpGet("keys/{apiKeyId}/records")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<UsageRecordResponse>>> GetKeyRecords(
        string apiKeyId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int limit = 100)
    {
        if (string.IsNullOrWhiteSpace(apiKeyId))
            return BadRequest(new { error = "API Key ID is required" });

        var start = startDate ?? DateTime.UtcNow.AddDays(-7);
        var end = endDate ?? DateTime.UtcNow;

        if (end < start)
            return BadRequest(new { error = "End date must be after start date" });

        try
        {
            var records = await _usageService.GetUsageRecordsAsync(apiKeyId, start, end);
            var response = records.Take(limit).Select(r => new UsageRecordResponse
            {
                Id = r.Id,
                RecordedAt = r.RecordedAt,
                Endpoint = r.Endpoint,
                Method = r.Method,
                StatusCode = r.ResponseStatusCode,
                RequestBytes = r.RequestBytes,
                ResponseBytes = r.ResponseBytes,
                ResponseTimeMs = r.ResponseTimeMs,
                SourceIp = r.SourceIp
            }).ToList();

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving usage records");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to retrieve usage records" });
        }
    }

    /// <summary>
    /// Retrieves aggregated usage for a consumer
    /// </summary>
    [HttpGet("consumers/{consumerId}/total")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ConsumerUsageResponse>> GetConsumerUsage(
        string consumerId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        if (string.IsNullOrWhiteSpace(consumerId))
            return BadRequest(new { error = "Consumer ID is required" });

        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        if (end < start)
            return BadRequest(new { error = "End date must be after start date" });

        try
        {
            var totalBytes = await _usageService.GetTotalBytesUsedAsync(consumerId, start, end);
            _logger.LogInformation("Retrieved usage total for consumer {ConsumerId}", consumerId);

            return Ok(new ConsumerUsageResponse
            {
                ConsumerId = consumerId,
                StartDate = start,
                EndDate = end,
                TotalBytesTransferred = totalBytes,
                TotalGBTransferred = Math.Round(totalBytes / (1024.0 * 1024.0 * 1024.0), 2)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving consumer usage");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to retrieve consumer usage" });
        }
    }
}

/// <summary>
/// Response model for usage statistics
/// </summary>
public class UsageStatisticsResponse
{
    public string ApiKeyId { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalRequests { get; set; }
    public int SuccessfulRequests { get; set; }
    public int FailedRequests { get; set; }
    public double SuccessRate { get; set; }
    public long TotalBytesTransferred { get; set; }
    public double AverageResponseTimeMs { get; set; }
    public int UniqueEndpoints { get; set; }
}

/// <summary>
/// Response model for a single usage record
/// </summary>
public class UsageRecordResponse
{
    public string Id { get; set; } = string.Empty;
    public DateTime RecordedAt { get; set; }
    public string Endpoint { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public long RequestBytes { get; set; }
    public long ResponseBytes { get; set; }
    public int ResponseTimeMs { get; set; }
    public string? SourceIp { get; set; }
}

/// <summary>
/// Response model for consumer usage
/// </summary>
public class ConsumerUsageResponse
{
    public string ConsumerId { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public long TotalBytesTransferred { get; set; }
    public double TotalGBTransferred { get; set; }
}
