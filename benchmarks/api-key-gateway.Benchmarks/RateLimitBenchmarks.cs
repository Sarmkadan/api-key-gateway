// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using ApiKeyGateway.Utilities;
using ApiKeyGateway.Domain.Enums;

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
        RateLimitCalculationHelper.GetWindowEnd(Now, RateLimitUnit.MINUTE);

    [Benchmark]
    public DateTime GetWindowEnd_Hour() =>
        RateLimitCalculationHelper.GetWindowEnd(Now, RateLimitUnit.HOUR);

    [Benchmark]
    public DateTime GetWindowStart_Minute() =>
        RateLimitCalculationHelper.GetWindowStart(Now, RateLimitUnit.MINUTE);

    [Benchmark]
    public int GetSecondsUntilAllowed_Limited() =>
        RateLimitCalculationHelper.GetSecondsUntilAllowed(100, 100, Now, RateLimitUnit.MINUTE);

    [Benchmark]
    public int CalculateQuotagePercentage() =>
        RateLimitCalculationHelper.CalculateQuotagePercentage(75, 100);
}
