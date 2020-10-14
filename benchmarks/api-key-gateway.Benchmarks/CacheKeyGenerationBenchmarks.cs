// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using ApiKeyGateway.Caching;

namespace ApiKeyGateway.Benchmarks;

/// <summary>
/// Benchmarks for cache key generation — a hot path executed on every authenticated request.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class CacheKeyGenerationBenchmarks
{
    private const string ApiKeyId = "ak_9f3e5b2d1a7c4e6f8b0d2a4c6e8f0b2d";
    private const string Endpoint = "/api/v1/data";

    private static readonly Dictionary<string, string> _threeParams = new()
    {
        ["version"] = "v1",
        ["region"] = "us-east-1",
        ["format"] = "json"
    };

    private static readonly Dictionary<string, string> _sixParams = new()
    {
        ["version"] = "v1",
        ["region"] = "us-east-1",
        ["format"] = "json",
        ["locale"] = "en-US",
        ["page"] = "1",
        ["limit"] = "100"
    };

    [Benchmark(Baseline = true)]
    public string RateLimitKey() =>
        CacheKeyGenerator.GetRateLimitKey(ApiKeyId, Endpoint);

    [Benchmark]
    public string ApiKeyKey() =>
        CacheKeyGenerator.GetApiKeyKey(ApiKeyId);

    [Benchmark]
    public string ApiKeyMetadataKey() =>
        CacheKeyGenerator.GetApiKeyMetadataKey(ApiKeyId);

    [Benchmark]
    public string QuotaKey() =>
        CacheKeyGenerator.GetQuotaKey(ApiKeyId);

    [Benchmark]
    public string ExternalApiKey_NoParams() =>
        CacheKeyGenerator.GetExternalApiCacheKey("stripe", "/v1/customers");

    [Benchmark]
    public string ExternalApiKey_ThreeParams() =>
        CacheKeyGenerator.GetExternalApiCacheKey("stripe", "/v1/customers", _threeParams);

    [Benchmark]
    public string ExternalApiKey_SixParams() =>
        CacheKeyGenerator.GetExternalApiCacheKey("stripe", "/v1/customers", _sixParams);
}
