// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiKeyGateway.Controllers;

/// <summary>
/// Provides aggregated usage analytics for API keys.
/// All date parameters are treated as UTC.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AnalyticsController : ControllerBase
{
    private readonly IUsageAnalyticsService _analytics;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(IUsageAnalyticsService analytics, ILogger<AnalyticsController> logger)
    {
        _analytics = analytics ?? throw new ArgumentNullException(nameof(analytics));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Returns a high-level usage summary for an API key over a date range.
    /// </summary>
    /// <param name="keyId">The API key ID to query.</param>
    /// <param name="from">Start of the date range (UTC). Defaults to 30 days ago.</param>
    /// <param name="to">End of the date range (UTC). Defaults to now.</param>
    [HttpGet("summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AnalyticsSummary>> GetSummary(
        [FromQuery] string keyId,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        if (string.IsNullOrWhiteSpace(keyId))
            return BadRequest(new { error = "keyId is required" });

        var (start, end, validationError) = ResolveDateRange(from, to);
        if (validationError is not null)
            return BadRequest(new { error = validationError });

        try
        {
            var summary = await _analytics.GetSummaryAsync(keyId, start, end);
            _logger.LogInformation("Analytics summary requested for key {KeyId}", keyId);
            return Ok(summary);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Returns the top N most-called endpoints for an API key.
    /// </summary>
    /// <param name="keyId">The API key ID to query.</param>
    /// <param name="limit">Maximum number of endpoints to return (default: 10, max: 100).</param>
    /// <param name="from">Start of the date range (UTC). Defaults to 30 days ago.</param>
    /// <param name="to">End of the date range (UTC). Defaults to now.</param>
    [HttpGet("top-endpoints")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<EndpointStat>>> GetTopEndpoints(
        [FromQuery] string keyId,
        [FromQuery] int limit = 10,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        if (string.IsNullOrWhiteSpace(keyId))
            return BadRequest(new { error = "keyId is required" });

        if (limit < 1 || limit > 100)
            return BadRequest(new { error = "limit must be between 1 and 100" });

        var (start, end, validationError) = ResolveDateRange(from, to);
        if (validationError is not null)
            return BadRequest(new { error = validationError });

        try
        {
            var endpoints = await _analytics.GetTopEndpointsAsync(keyId, start, end, limit);
            _logger.LogInformation(
                "Top-endpoints analytics requested for key {KeyId} (limit: {Limit})", keyId, limit);
            return Ok(endpoints);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Returns per-hour request counts and latency for an API key.
    /// </summary>
    /// <param name="keyId">The API key ID to query.</param>
    /// <param name="from">Start of the date range (UTC). Defaults to 24 hours ago.</param>
    /// <param name="to">End of the date range (UTC). Defaults to now.</param>
    [HttpGet("trends/hourly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<HourlyBucket>>> GetHourlyTrend(
        [FromQuery] string keyId,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        if (string.IsNullOrWhiteSpace(keyId))
            return BadRequest(new { error = "keyId is required" });

        var start = from?.ToUniversalTime() ?? DateTime.UtcNow.AddHours(-24);
        var end = to?.ToUniversalTime() ?? DateTime.UtcNow;

        if (end < start)
            return BadRequest(new { error = "to must be after from" });

        try
        {
            var buckets = await _analytics.GetHourlyTrendAsync(keyId, start, end);
            _logger.LogInformation("Hourly trend requested for key {KeyId}", keyId);
            return Ok(buckets);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Returns per-day request counts, error counts, latency and bytes transferred.
    /// </summary>
    /// <param name="keyId">The API key ID to query.</param>
    /// <param name="from">Start of the date range (UTC). Defaults to 30 days ago.</param>
    /// <param name="to">End of the date range (UTC). Defaults to now.</param>
    [HttpGet("trends/daily")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<DailyBucket>>> GetDailyTrend(
        [FromQuery] string keyId,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        if (string.IsNullOrWhiteSpace(keyId))
            return BadRequest(new { error = "keyId is required" });

        var (start, end, validationError) = ResolveDateRange(from, to);
        if (validationError is not null)
            return BadRequest(new { error = validationError });

        try
        {
            var buckets = await _analytics.GetDailyTrendAsync(keyId, start, end);
            _logger.LogInformation("Daily trend requested for key {KeyId}", keyId);
            return Ok(buckets);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Resolves a date range, applying sensible defaults when not provided.
    /// Returns (start, end, errorMessage).
    /// </summary>
    private static (DateTime start, DateTime end, string? error) ResolveDateRange(
        DateTime? from, DateTime? to)
    {
        var end = to?.ToUniversalTime() ?? DateTime.UtcNow;
        var start = from?.ToUniversalTime() ?? end.AddDays(-30);

        if (end < start)
            return (start, end, "to must be after from");

        return (start, end, null);
    }
}
