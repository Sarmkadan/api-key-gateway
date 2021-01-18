// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Threading.Tasks;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Provides extension methods for <see cref="UsageAnalyticsServiceTests"/> to facilitate test execution.
/// </summary>
public static class UsageAnalyticsServiceTestsExtensions
{
    /// <summary>
    /// Executes the summary tests for the provided <see cref="UsageAnalyticsServiceTests"/> instance.
    /// </summary>
    /// <param name="sut">The <see cref="UsageAnalyticsServiceTests"/> instance.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sut"/> is null.</exception>
    public static async Task RunSummaryTests(this UsageAnalyticsServiceTests sut)
    {
        ArgumentNullException.ThrowIfNull(sut);
        await sut.GetSummaryAsync_EmptyKeyId_ThrowsArgumentException();
        await sut.GetSummaryAsync_EndBeforeStart_ThrowsArgumentException();
        await sut.GetSummaryAsync_NoRecords_ReturnsZeroMetrics();
        await sut.GetSummaryAsync_MixedRecords_CalculatesMetricsCorrectly();
    }

    /// <summary>
    /// Executes the top endpoints tests for the provided <see cref="UsageAnalyticsServiceTests"/> instance.
    /// </summary>
    /// <param name="sut">The <see cref="UsageAnalyticsServiceTests"/> instance.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sut"/> is null.</exception>
    public static async Task RunTopEndpointsTests(this UsageAnalyticsServiceTests sut)
    {
        ArgumentNullException.ThrowIfNull(sut);
        await sut.GetTopEndpointsAsync_ReturnsEndpointsOrderedByCount();
        await sut.GetTopEndpointsAsync_LimitRespected();
    }

    /// <summary>
    /// Executes the trend tests for the provided <see cref="UsageAnalyticsServiceTests"/> instance.
    /// </summary>
    /// <param name="sut">The <see cref="UsageAnalyticsServiceTests"/> instance.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sut"/> is null.</exception>
    public static async Task RunTrendTests(this UsageAnalyticsServiceTests sut)
    {
        ArgumentNullException.ThrowIfNull(sut);
        await sut.GetHourlyTrendAsync_GroupsByHour();
        await sut.GetDailyTrendAsync_GroupsByDay();
    }

    /// <summary>
    /// Executes all tests for the provided <see cref="UsageAnalyticsServiceTests"/> instance.
    /// </summary>
    /// <param name="sut">The <see cref="UsageAnalyticsServiceTests"/> instance.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sut"/> is null.</exception>
    public static async Task RunAllTests(this UsageAnalyticsServiceTests sut)
    {
        ArgumentNullException.ThrowIfNull(sut);
        await sut.RunSummaryTests();
        await sut.RunTopEndpointsTests();
        await sut.RunTrendTests();
    }
}
