// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Domain.Models;
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
    private readonly IUsageQuotaService _usageQuotaService;
    private readonly ILogger<UsageController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UsageController"/> class
    /// </summary>
    /// <param name="usageService">Service for usage tracking and statistics</param>
    /// <param name="usageQuotaService">Service for quota consumption lookups</param>
    /// <param name="logger">Logger for controller diagnostics</param>
    public UsageController(
        IUsageTrackingService usageService,
        IUsageQuotaService usageQuotaService,
        ILogger<UsageController> logger)
    {
        _usageService = usageService ?? throw new ArgumentNullException(nameof(usageService));
        _usageQuotaService = usageQuotaService ?? throw new ArgumentNullException(nameof(usageQuotaService));
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
        {
            return BadRequest(new { error = "API Key ID is required" });
        }

        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        if (end < start)
        {
            return BadRequest(new { error = "End date must be after start date" });
        }

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
        {
            return BadRequest(new { error = "API Key ID is required" });
        }

        var start = startDate ?? DateTime.UtcNow.AddDays(-7);
        var end = endDate ?? DateTime.UtcNow;

        if (end < start)
        {
            return BadRequest(new { error = "End date must be after start date" });
        }

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
        {
            return BadRequest(new { error = "Consumer ID is required" });
        }

        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        if (end < start)
        {
            return BadRequest(new { error = "End date must be after start date" });
        }

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

    /// <summary>
    /// Retrieves current quota consumption for an API key
    /// </summary>
    [HttpGet("quota/{keyId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuotaConsumptionResponse>> GetQuotaConsumption(
        string keyId)
    {
        if (string.IsNullOrWhiteSpace(keyId))
        {
            return BadRequest(new { error = "Key ID is required" });
        }

        try
        {
            var quota = await _usageQuotaService.GetQuotaAsync(keyId);

            if (quota == null)
            {
                return NotFound(new { error = "Quota not configured for this key" });
            }

            var periodEnd = quota.GetPeriodEndUtc();
            var isExceeded = quota.IsExceeded;
            var remaining = quota.RemainingRequests;
            var limit = quota.QuotaLimit;
            var percent = limit > 0 ? Math.Round(((double)(limit - remaining) / limit) * 100, 2) : 0;

            _logger.LogInformation("Retrieved quota consumption for API key {ApiKeyId}", keyId);

            return Ok(new QuotaConsumptionResponse
            {
                KeyId = keyId,
                Used = quota.CurrentUsage,
                Limit = quota.QuotaLimit,
                PercentUsed = percent,
                IsExceeded = isExceeded,
                Period = quota.Period.ToString(),
                PeriodStart = quota.PeriodStartAt,
                PeriodEnd = periodEnd,
                Remaining = remaining
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving quota consumption");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to retrieve quota consumption" });
        }
    }
}

/// <summary>
/// Response model for usage statistics
/// </summary>
public class UsageStatisticsResponse
{
    /// <summary>
    /// Gets or sets the API key identifier
    /// </summary>
    public string ApiKeyId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the start of the reporting period
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Gets or sets the end of the reporting period
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Gets or sets the total number of requests
    /// </summary>
    public int TotalRequests { get; set; }

    /// <summary>
    /// Gets or sets the number of successful requests
    /// </summary>
    public int SuccessfulRequests { get; set; }

    /// <summary>
    /// Gets or sets the number of failed requests
    /// </summary>
    public int FailedRequests { get; set; }

    /// <summary>
    /// Gets or sets the success rate of requests
    /// </summary>
    public double SuccessRate { get; set; }

    /// <summary>
    /// Gets or sets the total bytes transferred
    /// </summary>
    public long TotalBytesTransferred { get; set; }

    /// <summary>
    /// Gets or sets the average response time in milliseconds
    /// </summary>
    public double AverageResponseTimeMs { get; set; }

    /// <summary>
    /// Gets or sets the number of unique endpoints accessed
    /// </summary>
    public int UniqueEndpoints { get; set; }
}

/// <summary>
/// Response model for a single usage record
/// </summary>
public class UsageRecordResponse
{
    /// <summary>
    /// Gets or sets the record identifier
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the time the request was recorded
    /// </summary>
    public DateTime RecordedAt { get; set; }

    /// <summary>
    /// Gets or sets the endpoint that was called
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the HTTP method of the request
    /// </summary>
    public string Method { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the HTTP status code of the response
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the size of the request in bytes
    /// </summary>
    public long RequestBytes { get; set; }

    /// <summary>
    /// Gets or sets the size of the response in bytes
    /// </summary>
    public long ResponseBytes { get; set; }

    /// <summary>
    /// Gets or sets the response time in milliseconds
    /// </summary>
    public int ResponseTimeMs { get; set; }

    /// <summary>
    /// Gets or sets the source IP address of the request
    /// </summary>
    public string? SourceIp { get; set; }
}

/// <summary>
/// Response model for consumer usage
/// </summary>
public class ConsumerUsageResponse
{
    /// <summary>
    /// Gets or sets the consumer identifier
    /// </summary>
    public string ConsumerId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the start of the reporting period
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Gets or sets the end of the reporting period
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Gets or sets the total bytes transferred
    /// </summary>
    public long TotalBytesTransferred { get; set; }

    /// <summary>
    /// Gets or sets the total bytes transferred expressed in gigabytes
    /// </summary>
    public double TotalGBTransferred { get; set; }
}

/// <summary>
/// Response model for quota consumption
/// </summary>
public class QuotaConsumptionResponse
{
    /// <summary>
    /// Gets or sets the API key identifier
    /// </summary>
    public string KeyId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the amount of quota used in the current period
    /// </summary>
    public long Used { get; set; }

    /// <summary>
    /// Gets or sets the quota limit for the period
    /// </summary>
    public long Limit { get; set; }

    /// <summary>
    /// Gets or sets the percentage of quota used
    /// </summary>
    public double PercentUsed { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the quota has been exceeded
    /// </summary>
    public bool IsExceeded { get; set; }

    /// <summary>
    /// Gets or sets the quota period name
    /// </summary>
    public string Period { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the start of the current quota period
    /// </summary>
    public DateTime PeriodStart { get; set; }

    /// <summary>
    /// Gets or sets the end of the current quota period
    /// </summary>
    public DateTime PeriodEnd { get; set; }

    /// <summary>
    /// Gets or sets the number of requests remaining in the period
    /// </summary>
    public long Remaining { get; set; }
}