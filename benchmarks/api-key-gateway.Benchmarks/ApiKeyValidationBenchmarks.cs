// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using ApiKeyGateway.Validation;

namespace ApiKeyGateway.Benchmarks;

/// <summary>
/// Benchmarks for API key and name validation.
/// ValidateKeyFormat uses a single-pass span scan to detect entropy in one traversal.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class ApiKeyValidationBenchmarks
{
    // Realistic 32-char key with mixed character types
    private const string ValidKey32 = "sK_Ab1!xYz9#QrWe4$TyUi7^OpLm2@Vb";

    // Valid 64-char key — common production key length
    private const string ValidKey64 =
        "sK_Ab1!xYz9#QrWe4$TyUi7^OpLm2@VbNc3%HjPk6&GfDs8*ZxCv0(BnMq5)Rt";

    // Only lowercase — fails entropy check (single pass returns early)
    private const string WeakKey = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

    // Too short — fails length check before any character scan
    private const string ShortKey = "short123";

    [Benchmark(Baseline = true)]
    public ValidationResult ValidateFormat_32Char_Valid() =>
        ApiKeyValidator.ValidateKeyFormat(ValidKey32);

    [Benchmark]
    public ValidationResult ValidateFormat_64Char_Valid() =>
        ApiKeyValidator.ValidateKeyFormat(ValidKey64);

    [Benchmark]
    public ValidationResult ValidateFormat_WeakEntropy() =>
        ApiKeyValidator.ValidateKeyFormat(WeakKey);

    [Benchmark]
    public ValidationResult ValidateFormat_TooShort() =>
        ApiKeyValidator.ValidateKeyFormat(ShortKey);

    [Benchmark]
    public ValidationResult ValidateName_Valid() =>
        ApiKeyValidator.ValidateKeyName("Production API Key");

    [Benchmark]
    public ValidationResult ValidateName_TooLong() =>
        ApiKeyValidator.ValidateKeyName(new string('a', 110));

    [Benchmark]
    public ValidationResult ValidateQuota_Valid() =>
        ApiKeyValidator.ValidateQuotaLimit(10_000);
}
