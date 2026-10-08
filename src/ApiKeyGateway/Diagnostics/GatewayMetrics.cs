// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics.Metrics;

namespace ApiKeyGateway.Diagnostics;

/// <summary>
/// Request outcome counters exposed through a <see cref="Meter"/> named <see cref="MeterName"/>.
/// Register the meter with your metrics exporter, e.g. <c>AddMeter("ApiKeyGateway")</c> for OpenTelemetry.
/// Counters carry no API key identifiers, so cardinality stays bounded.
/// </summary>
public static class GatewayMetrics
{
    /// <summary>Name of the meter that owns the gateway counters.</summary>
    public const string MeterName = "ApiKeyGateway";

    private const string MeterVersion = "1.0.0";

    private static readonly Meter _meter = new(MeterName, MeterVersion);

    /// <summary>Requests admitted past authentication, rate limiting, scope and quota checks.</summary>
    public static readonly Counter<long> RequestsAllowed = _meter.CreateCounter<long>(
        "requests_allowed",
        unit: "{request}",
        description: "Requests admitted by the gateway");

    /// <summary>Requests rejected because the API key exceeded its rate limit.</summary>
    public static readonly Counter<long> RequestsRateLimited = _meter.CreateCounter<long>(
        "requests_rate_limited",
        unit: "{request}",
        description: "Requests rejected by rate limiting");

    /// <summary>Requests rejected during authentication. Tagged with <c>reason</c>.</summary>
    public static readonly Counter<long> RequestsUnauthorized = _meter.CreateCounter<long>(
        "requests_unauthorized",
        unit: "{request}",
        description: "Requests rejected by authentication");

    /// <summary>
    /// Records a rejected authentication attempt, tagged with the failure reason.
    /// </summary>
    /// <param name="reason">Failure reason name, e.g. <c>ApiKeyExpired</c>. Must come from a fixed set.</param>
    public static void RecordUnauthorized(string reason)
    {
        RequestsUnauthorized.Add(1, new KeyValuePair<string, object?>("reason", reason));
    }
}
