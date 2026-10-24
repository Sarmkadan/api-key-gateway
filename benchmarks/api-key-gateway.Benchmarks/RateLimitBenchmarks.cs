// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Domain.Enums;
using ApiKeyGateway.Utilities;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace ApiKeyGateway.Benchmarks;

/// <summary>
/// Benchmarks for rate limit calculation helpers.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class RateLimitBenchmarks
{
    private static readonly DateTime Now = DateTime.UtcNow;

    [Benchmark(Baseline = true)]
    public DateTime GetWindowEnd_Minute() =>
        RateLimitCalculationHelper.GetWindowEnd(Now, RateLimitUnit.Minute);

    [Benchmark]
    public DateTime GetWindowEnd_Hour() =>
        RateLimitCalculationHelper.GetWindowEnd(Now, RateLimitUnit.Hour);

    [Benchmark]
    public DateTime GetWindowStart_Minute() =>
        RateLimitCalculationHelper.GetWindowStart(Now, RateLimitUnit.Minute);

    [Benchmark]
    public int GetSecondsUntilAllowed_Limited() =>
        RateLimitCalculationHelper.GetSecondsUntilAllowed(100, 100, Now, RateLimitUnit.Minute);

    [Benchmark]
    public int CalculateQuotagePercentage() =>
        RateLimitCalculationHelper.CalculateQuotagePercentage(75, 100);
}
