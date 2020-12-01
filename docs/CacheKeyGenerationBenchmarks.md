# CacheKeyGenerationBenchmarks

The `CacheKeyGenerationBenchmarks` class serves as a dedicated benchmark suite for measuring the performance and consistency of cache key generation strategies within the `api-key-gateway` project. It exposes a set of pre-computed or generated string constants representing various caching scenarios, including rate limiting, API key validation, metadata retrieval, and quota enforcement. These members are utilized by benchmarking frameworks to evaluate the overhead of key construction under different parameter loads, ensuring that the gateway's caching layer remains efficient during high-throughput operations.

## API

### RateLimitKey
A public string field representing the standardized cache key pattern used for rate limiting operations. This key typically incorporates client identifiers and time-window buckets to enforce request throttling. It does not accept parameters and does not throw exceptions, as it serves as a static reference or template for key construction logic during benchmarking.

### ApiKeyKey
A public string field defining the cache key structure for primary API key lookups. This member is used to verify the performance of retrieving core authentication credentials from the cache. It is a read-only string value intended for comparison or template usage within benchmark iterations and does not involve dynamic parameter injection or exception throwing in this context.

### ApiKeyMetadataKey
A public string field specifying the cache key format for retrieving auxiliary metadata associated with an API key, such as owner details, permission scopes, or expiration policies. This member allows benchmarks to isolate the cost of fetching extended key attributes separate from the key validation itself. It is a constant string reference and does not throw exceptions.

### QuotaKey
A public string field representing the cache key pattern used for tracking and enforcing usage quotas. This key is essential for benchmarks measuring the performance of counter increments and quota limit checks. As a string member defining a pattern or static value, it requires no parameters and poses no risk of runtime exceptions during access.

### ExternalApiKey_NoParams
A public string field containing a generated or static cache key for external API key validation scenarios that require no additional parameters. This member establishes a baseline performance metric for the simplest possible key generation path. It is a direct string value, requiring no arguments and guaranteed not to throw exceptions upon access.

### ExternalApiKey_ThreeParams
A public string field representing a cache key constructed for external API key validations involving three distinct parameters (e.g., key ID, tenant ID, and resource scope). This member is used to benchmark the serialization and hashing overhead associated with moderate-complexity key signatures. It is exposed as a string result and does not accept further parameters or throw exceptions.

### ExternalApiKey_SixParams
A public string field holding a cache key designed for complex external API key validation scenarios requiring six parameters. This member stress-tests the key generation logic under high-cardinality input conditions to identify potential bottlenecks in string concatenation or hashing algorithms. It is a string value provided for benchmark comparison, with no parameters and no exception risks.

## Usage

### Example 1: Baseline Performance Comparison
The following example demonstrates how to use the benchmark members to compare the raw access time of simple versus complex key patterns within a benchmarking loop.

```csharp
using System;
using api_key_gateway.Benchmarks;

public class KeyComplexityTest
{
    public void RunComparison()
    {
        var benchmarks = new CacheKeyGenerationBenchmarks();
        
        // Simulate accessing simple key patterns
        var simpleKey = benchmarks.ExternalApiKey_NoParams;
        Console.WriteLine($"Simple Key: {simpleKey}");

        // Simulate accessing complex key patterns
        var complexKey = benchmarks.ExternalApiKey_SixParams;
        Console.WriteLine($"Complex Key: {complexKey}");

        // In a real benchmark, these accesses would be timed within a loop
        // to measure the overhead of key complexity on the CPU cache.
    }
}
```

### Example 2: Validating Key Pattern Consistency
This example illustrates verifying that the specific key types (RateLimit, Quota, Metadata) are correctly populated before running a load test against the gateway.

```csharp
using System;
using System.Collections.Generic;
using api_key_gateway.Benchmarks;

public class CacheKeyValidator
{
    public void ValidateKeyTemplates()
    {
        var benchmarks = new CacheKeyGenerationBenchmarks();
        var keys = new List<string>
        {
            benchmarks.RateLimitKey,
            benchmarks.ApiKeyKey,
            benchmarks.ApiKeyMetadataKey,
            benchmarks.QuotaKey
        };

        foreach (var key in keys)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new InvalidOperationException("Cache key template is missing.");
            }
            // Log or assert key format compliance here
            Console.WriteLine($"Validated Key Pattern: {key}");
        }
    }
}
```

## Notes

*   **Thread Safety**: As the members of `CacheKeyGenerationBenchmarks` are exposed as public fields (likely immutable strings or constants once initialized), they are inherently thread-safe for read operations. Multiple threads can access `RateLimitKey`, `QuotaKey`, or any other member simultaneously without requiring locking mechanisms, provided the instance itself is not being modified during execution (which is typical for benchmark classes).
*   **Edge Cases**: Since these members represent specific parameter counts (0, 3, and 6), they do not dynamically handle variable argument lists. If the underlying key generation logic changes to support a different number of parameters, these specific benchmark members (`ExternalApiKey_ThreeParams`, etc.) may no longer reflect production reality and should be updated to match the new signature requirements.
*   **Null Handling**: While the members are intended to be populated strings, consumers should treat them as potentially null if the benchmark initialization fails, although standard usage implies they are pre-computed during object construction. The provided usage examples include basic validation to guard against empty keys.
*   **Purpose Limitation**: This class is designed strictly for benchmarking and performance analysis. It should not be used as the source of truth for actual cache key generation in production request pipelines, as the strings may represent templates or specific test cases rather than dynamic generation logic.
