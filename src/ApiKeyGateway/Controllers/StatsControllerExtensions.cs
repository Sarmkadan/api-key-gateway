// =============================================================================
// Author: Automated Extension Generator
// =====================================================================

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace ApiKeyGateway.Controllers;

/// <summary>
/// Extension methods that provide strongly‑typed access to the data returned by
/// <see cref="StatsController"/> actions. The controller itself only returns
/// <see cref="IActionResult"/> instances that wrap anonymous objects; these
/// helpers deserialize the payload into concrete DTOs that can be used by
/// callers without resorting to reflection or dynamic typing.
/// </summary>
public static class StatsControllerExtensions
{
    /// <summary>
    /// Retrieves usage statistics for the specified <paramref name="period"/> as a
    /// strongly‑typed <see cref="UsageStatsDto"/>.
    /// </summary>
    /// <param name="controller">The <see cref="StatsController"/> instance.</param>
    /// <param name="period">
    /// The period to request (e.g., <c>hour</c>, <c>day</c>, <c>month</c>).
    /// Defaults to <c>day</c>.
    /// </param>
    /// <returns>A <see cref="UsageStatsDto"/> containing the statistics.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="controller"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="period"/> is <c>null</c> or empty.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the controller does not return an <see cref="ObjectResult"/>
    /// or the payload is not of the expected type.
    /// </exception>
    public static UsageStatsDto GetUsageStatisticsDto(this StatsController controller, string period = "day")
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentException.ThrowIfNullOrEmpty(period);

        var result = controller.GetUsageStatistics(period);
        return result switch
        {
            ObjectResult { Value: UsageStatsDto dto } => dto,
            ObjectResult { Value: not null } => throw new InvalidOperationException(
                "Controller returned unexpected type for usage statistics."),
            _ => throw new InvalidOperationException(
                "Unexpected result type from GetUsageStatistics.")
        };
    }

    /// <summary>
    /// Retrieves the current rate‑limit status for the authenticated API key as a
    /// strongly‑typed <see cref="RateLimitStatusDto"/>.
    /// </summary>
    /// <param name="controller">The <see cref="StatsController"/> instance.</param>
    /// <returns>A <see cref="RateLimitStatusDto"/> representing the rate‑limit data.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="controller"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the controller does not return an <see cref="ObjectResult"/>
    /// or the payload is not of the expected type.
    /// </exception>
    public static RateLimitStatusDto GetRateLimitStatusDto(this StatsController controller)
    {
        ArgumentNullException.ThrowIfNull(controller);

        var result = controller.GetRateLimitStatus();
        return result switch
        {
            ObjectResult { Value: RateLimitStatusDto dto } => dto,
            ObjectResult { Value: not null } => throw new InvalidOperationException(
                "Controller returned unexpected type for rate-limit status."),
            _ => throw new InvalidOperationException(
                "Unexpected result type from GetRateLimitStatus.")
        };
    }

    /// <summary>
    /// Retrieves endpoint‑specific statistics as a read‑only list of
    /// <see cref="EndpointStatDto"/>.
    /// </summary>
    /// <param name="controller">The <see cref="StatsController"/> instance.</param>
    /// <returns>
    /// An <see cref="IReadOnlyList{T}"/> of <see cref="EndpointStatDto"/> objects.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="controller"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the controller does not return an <see cref="ObjectResult"/>
    /// or the payload is not of the expected type.
    /// </exception>
    public static IReadOnlyList<EndpointStatDto> GetEndpointStatisticsList(this StatsController controller)
    {
        ArgumentNullException.ThrowIfNull(controller);

        var result = controller.GetEndpointStatistics();
        return result switch
        {
            ObjectResult { Value: EndpointStatsWrapper { Endpoints: var endpoints } } => endpoints,
            ObjectResult { Value: not null } => throw new InvalidOperationException(
                "Controller returned unexpected type for endpoint statistics."),
            _ => throw new InvalidOperationException(
                "Unexpected result type from GetEndpointStatistics.")
        };
    }

    // -------------------------------------------------------------------------
    // DTO definitions used by the extension methods.
    // -------------------------------------------------------------------------

    /// <summary>
    /// DTO representing usage statistics for a given period.
    /// </summary>
    public sealed record UsageStatsDto(
        string Period,
        int Requests,
        int Errors,
        double TotalDataTransferred,
        double AverageResponseTime);

    /// <summary>
    /// DTO representing a single rate‑limit bucket.
    /// </summary>
    public sealed record RateLimitDto(
        int Limit,
        int Current,
        int Remaining,
        string ResetIn);

    /// <summary>
    /// DTO representing the full rate‑limit status payload.
    /// </summary>
    public sealed record RateLimitStatusDto(
        string ApiKeyId,
        RateLimitDto Hourly,
        RateLimitDto Daily,
        RateLimitDto Monthly,
        string Status);

    /// <summary>
    /// DTO representing statistics for a single endpoint.
    /// </summary>
    public sealed record EndpointStatDto(
        string Path,
        int Requests,
        int AvgResponseTime,
        int ErrorCount);

    /// <summary>
    /// Internal wrapper used to deserialize the anonymous object returned by
    /// <c>StatsController.GetEndpointStatistics</c>.
    /// </summary>
    private sealed record EndpointStatsWrapper(
        string ApiKeyId,
        IReadOnlyList<EndpointStatDto> Endpoints);
}