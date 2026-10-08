// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics.Metrics;
using ApiKeyGateway.Diagnostics;
using FluentAssertions;
using Xunit;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Verifies the gateway <see cref="Meter"/> and its request outcome counters.
/// The counters are process-wide, so assertions only look for measurements this test produced
/// (unique tag values or a minimum count), which keeps them stable when other tests run in parallel.
/// </summary>
public class GatewayMetricsTests
{
    /// <summary>
    /// Verifies that the counters use the documented instrument names and the meter uses the documented name.
    /// </summary>
    [Fact]
    public void GatewayMetrics_InstrumentNames_MatchDocumentedNames()
    {
        // Arrange & Act
        var names = new[]
        {
            GatewayMetrics.RequestsAllowed.Name,
            GatewayMetrics.RequestsRateLimited.Name,
            GatewayMetrics.RequestsUnauthorized.Name
        };

        // Assert
        GatewayMetrics.MeterName.Should().Be("ApiKeyGateway");
        names.Should().BeEquivalentTo(new[] { "requests_allowed", "requests_rate_limited", "requests_unauthorized" });
    }

    /// <summary>
    /// Verifies that <see cref="GatewayMetrics.RecordUnauthorized"/> publishes one measurement tagged with the reason.
    /// </summary>
    [Fact]
    public void RecordUnauthorized_WithReason_PublishesTaggedMeasurement()
    {
        // Arrange
        var reason = "TestReason-" + Guid.NewGuid();
        var received = new List<(string Instrument, long Value, object? Reason)>();
        using var listener = CreateListener(received);

        // Act
        GatewayMetrics.RecordUnauthorized(reason);

        // Assert
        received.Should().ContainSingle(m =>
            m.Instrument == "requests_unauthorized" && m.Value == 1 && Equals(m.Reason, reason));
    }

    /// <summary>
    /// Verifies that <see cref="GatewayMetrics.RequestsAllowed"/> publishes a measurement of one.
    /// </summary>
    [Fact]
    public void RequestsAllowed_Add_PublishesMeasurement()
    {
        // Arrange
        var received = new List<(string Instrument, long Value, object? Reason)>();
        using var listener = CreateListener(received);

        // Act
        GatewayMetrics.RequestsAllowed.Add(1);

        // Assert
        received.Should().Contain(m => m.Instrument == "requests_allowed" && m.Value == 1);
    }

    /// <summary>
    /// Verifies that <see cref="GatewayMetrics.RequestsRateLimited"/> publishes a measurement of one.
    /// </summary>
    [Fact]
    public void RequestsRateLimited_Add_PublishesMeasurement()
    {
        // Arrange
        var received = new List<(string Instrument, long Value, object? Reason)>();
        using var listener = CreateListener(received);

        // Act
        GatewayMetrics.RequestsRateLimited.Add(1);

        // Assert
        received.Should().Contain(m => m.Instrument == "requests_rate_limited" && m.Value == 1);
    }

    private static MeterListener CreateListener(List<(string Instrument, long Value, object? Reason)> sink)
    {
        var listener = new MeterListener
        {
            InstrumentPublished = (instrument, l) =>
            {
                if (instrument.Meter.Name == GatewayMetrics.MeterName)
                {
                    l.EnableMeasurementEvents(instrument);
                }
            }
        };

        listener.SetMeasurementEventCallback<long>((instrument, value, tags, _) =>
        {
            object? reason = null;
            foreach (var tag in tags)
            {
                if (tag.Key == "reason")
                {
                    reason = tag.Value;
                }
            }

            lock (sink)
            {
                sink.Add((instrument.Name, value, reason));
            }
        });

        listener.Start();
        return listener;
    }
}
