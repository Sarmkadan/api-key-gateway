# UsageQuotaServiceTestsExtensions

Helper class providing extension methods and utilities for testing `UsageQuotaService` functionality. These methods simplify the setup of mock repositories, service creation, and verification of quota operations in unit tests.

## API

### `SetupQuotaRepository`

Configures a mocked `IUsageQuotaRepository` with predefined quota values for testing purposes.

- **Parameters**
  - `mock`: The `Mock<IUsageQuotaRepository>` instance to configure.
  - `quotas`: A dictionary mapping quota keys to their respective limits. If `null`, the repository will return empty results.
- **Return Value**
  - The configured `Mock<IUsageQuotaRepository>` for method chaining.
- **Throws**
  - `ArgumentNullException` if `mock` is `null`.

### `CreateQuotaService`

Creates an instance of `UsageQuotaService` with the provided dependencies.

- **Parameters**
  - `quotaRepository`: The repository mock to inject into the service.
  - `logger`: Optional logger instance. If `null`, a default `NullLogger<UsageQuotaService>` is used.
- **Return Value**
  - A new `UsageQuotaService` instance ready for testing.
- **Throws**
  - `ArgumentNullException` if `quotaRepository` is `null`.

### `VerifyQuotaSet`

Verifies that the `SetQuotaAsync` method was called with the expected parameters.

- **Parameters**
  - `mock`: The mocked `IUsageQuotaRepository`.
  - `key`: The quota key expected to be set.
  - `limit`: The quota limit expected to be set.
- **Throws**
  - `MockException` if the method was not called with the specified parameters.

### `VerifyQuotaGet`

Verifies that the `GetQuotaAsync` method was called with the expected quota key.

- **Parameters**
  - `mock`: The mocked `IUsageQuotaRepository`.
  - `key`: The quota key expected to be retrieved.
- **Throws**
  - `MockException` if the method was not called with the specified key.

### `CreateQuotaKeys`

Generates a set of quota keys for testing.

- **Parameters**
  - `prefix`: Optional prefix for the keys. If `null` or empty, keys are generated without a prefix.
  - `count`: The number of keys to generate.
- **Return Value**
  - An `IReadOnlyDictionary<string, long>` mapping generated keys to their limits (default limit is `100`).
- **Throws**
  - `ArgumentOutOfRangeException` if `count` is less than `1`.

### `ParseQuotaLimit`

Parses a string representation of a quota limit into a nullable long.

- **Parameters**
  - `limitStr`: The string to parse (e.g., `"100"`, `"unlimited"`).
- **Return Value**
  - The parsed `long` value if successful, or `null` if the string is `"unlimited"` or cannot be parsed.
- **Throws**
  - `ArgumentNullException` if `limitStr` is `null`.

### `CreateCapturingLogger`

Creates a logger that captures log messages for verification in tests.

- **Parameters**
  - None.
- **Return Value**
  - A tuple containing:
    - `Mock<ILogger<UsageQuotaService>>`: The mock logger instance.
    - `List<string>`: The captured log messages.

## Usage

### Example 1: Testing quota retrieval and setting

```csharp
[Fact]
public async Task GetQuotaAsync_WhenQuotaExists_ReturnsLimit()
{
    // Arrange
    var mockRepo = new Mock<IUsageQuotaRepository>();
    var quotaKeys = UsageQuotaServiceTestsExtensions.CreateQuotaKeys("test", 1);
    UsageQuotaServiceTestsExtensions.SetupQuotaRepository(mockRepo, quotaKeys);

    var service = UsageQuotaServiceTestsExtensions.CreateQuotaService(mockRepo.Object);

    // Act
    var result = await service.GetQuotaAsync("test-0");

    // Assert
    Assert.Equal(100, result);
    UsageQuotaServiceTestsExtensions.VerifyQuotaGet(mockRepo, "test-0");
}
```

### Example 2: Testing quota updates

```csharp
[Fact]
public async Task SetQuotaAsync_UpdatesQuotaValue()
{
    // Arrange
    var mockRepo = new Mock<IUsageQuotaRepository>();
    var service = UsageQuotaServiceTestsExtensions.CreateQuotaService(mockRepo.Object);

    // Act
    await service.SetQuotaAsync("new-key", 500);

    // Assert
    UsageQuotaServiceTestsExtensions.VerifyQuotaSet(mockRepo, "new-key", 500);
}
```

## Notes

- **Thread Safety**: All methods are stateless and thread-safe. The generated mocks and services are not inherently thread-safe; ensure proper synchronization if shared across threads in tests.
- **Edge Cases**:
  - Methods accepting `null` parameters throw `ArgumentNullException` explicitly.
  - `ParseQuotaLimit` handles `"unlimited"` by returning `null` without throwing.
  - `CreateQuotaKeys` with `count = 0` throws `ArgumentOutOfRangeException`.
  - The `CreateCapturingLogger` captures messages in a thread-safe `List<string>`; however, concurrent logging operations may interleave messages.
