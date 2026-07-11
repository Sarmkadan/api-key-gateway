// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using ApiKeyGateway.Utilities;

namespace ApiKeyGateway.Benchmarks;

/// <summary>
/// Benchmarks for cryptographic helpers.
/// GenerateSecureRandomString uses RandomNumberGenerator.GetString for unbiased character selection.
/// ComputeSha256Hash and ComputeHmacSha256 use ArrayPool for encoding and stackalloc for hash output.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class CryptoBenchmarks
{
    private const string SampleKey =
        "sK_Ab1!xYz9#QrWe4$TyUi7^OpLm2@VbNc3%HjPk6&GfDs8";

    private const string WebhookSecret = "wh_secret_key_2026_prod";

    private const string WebhookPayload =
        "POST /api/webhooks 1714900200 {\"event\":\"key.created\",\"keyId\":\"ak_abc123\"}";

    private static readonly string _precomputedHash =
        CryptoHelpers.ComputeSha256Hash(SampleKey);

    [Benchmark(Baseline = true)]
    public string GenerateRandom_32() =>
        CryptoHelpers.GenerateSecureRandomString(32);

    [Benchmark]
    public string GenerateRandom_64() =>
        CryptoHelpers.GenerateSecureRandomString(64);

    [Benchmark]
    public string GenerateRandom_128() =>
        CryptoHelpers.GenerateSecureRandomString(128);

    [Benchmark]
    public string ComputeSha256() =>
        CryptoHelpers.ComputeSha256Hash(SampleKey);

    [Benchmark]
    public string ComputeHmac() =>
        CryptoHelpers.ComputeHmacSha256(WebhookPayload, WebhookSecret);

    [Benchmark]
    public bool VerifyHash() =>
        CryptoHelpers.VerifyHash(SampleKey, _precomputedHash);
}
