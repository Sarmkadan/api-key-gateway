namespace ApiKeyGateway.Tests;

using System.Reflection;
using Xunit;

/// <summary>
/// Provides validation helpers for <see cref="UsageTrackingServiceTests"/> test class.
/// Validates test method naming conventions, attributes, structure, and test coverage.
/// </summary>
public static class UsageTrackingServiceTestsValidation
{
    /// <summary>
    /// Validates the test class instance and returns a list of validation problems.
    /// </summary>
    /// <param name="value">The test class instance to validate.</param>
    /// <returns>Collection of human-readable validation problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this UsageTrackingServiceTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();
        var testClassType = value.GetType();

        // Validate test method attributes and naming conventions
        var testMethods = testClassType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.DeclaringType == testClassType)
            .Where(m => m.Name.StartsWith("Constructor_") ||
                       m.Name.StartsWith("RecordUsageAsync_") ||
                       m.Name.StartsWith("GetUsageStatisticsAsync_") ||
                       m.Name.StartsWith("GetUsageRecordsAsync_") ||
                       m.Name.StartsWith("GetTotalBytesUsedAsync_"))
            .ToList();

        if (testMethods.Count == 0)
        {
            problems.Add("Test class has no recognized test methods (Constructor_, RecordUsageAsync_, GetUsageStatisticsAsync_, GetUsageRecordsAsync_, or GetTotalBytesUsedAsync_ prefixes)");
        }

        var constructorTests = testMethods.Where(m => m.Name.StartsWith("Constructor_")).ToList();
        var recordUsageTests = testMethods.Where(m => m.Name.StartsWith("RecordUsageAsync_")).ToList();
        var getUsageStatsTests = testMethods.Where(m => m.Name.StartsWith("GetUsageStatisticsAsync_")).ToList();
        var getUsageRecordsTests = testMethods.Where(m => m.Name.StartsWith("GetUsageRecordsAsync_")).ToList();
        var getTotalBytesTests = testMethods.Where(m => m.Name.StartsWith("GetTotalBytesUsedAsync_")).ToList();

        // Validate constructor tests
        if (constructorTests.Count == 0)
        {
            problems.Add("Test class should include constructor validation tests (Constructor_NullRepository_ThrowsArgumentNullException, Constructor_NullLogger_ThrowsArgumentNullException)");
        }
        else
        {
            ValidateConstructorTests(constructorTests, problems);
        }

        // Validate RecordUsageAsync tests
        if (recordUsageTests.Count == 0)
        {
            problems.Add("Test class should include RecordUsageAsync tests (RecordUsageAsync_NullRecord_ThrowsArgumentNullException, RecordUsageAsync_ValidRecord_CreatesInRepository, RecordUsageAsync_RepositoryThrows_WrapsInDataAccessException)");
        }
        else
        {
            ValidateRecordUsageTests(recordUsageTests, problems);
        }

        // Validate GetUsageStatisticsAsync tests
        if (getUsageStatsTests.Count == 0)
        {
            problems.Add("Test class should include GetUsageStatisticsAsync tests (GetUsageStatisticsAsync_EmptyOrNullKeyId_ThrowsValidationException, GetUsageStatisticsAsync_EndDateBeforeStartDate_ThrowsValidationException, GetUsageStatisticsAsync_ValidDateRange_ReturnsStatisticsWithCorrectAggregates, GetUsageStatisticsAsync_NoRecords_ReturnsZeroedStatistics)");
        }
        else
        {
            ValidateGetUsageStatisticsTests(getUsageStatsTests, problems);
        }

        // Validate GetUsageRecordsAsync tests
        if (getUsageRecordsTests.Count == 0)
        {
            problems.Add("Test class should include GetUsageRecordsAsync tests (GetUsageRecordsAsync_EmptyOrNullKeyId_ThrowsValidationException, GetUsageRecordsAsync_ValidDateRange_ReturnsRecordsFromRepository)");
        }
        else
        {
            ValidateGetUsageRecordsTests(getUsageRecordsTests, problems);
        }

        // Validate GetTotalBytesUsedAsync tests
        if (getTotalBytesTests.Count == 0)
        {
            problems.Add("Test class should include GetTotalBytesUsedAsync tests (GetTotalBytesUsedAsync_EmptyOrNullConsumerId_ReturnsZero, GetTotalBytesUsedAsync_ValidConsumerId_ReturnsAggregatedBytes, GetTotalBytesUsedAsync_NoRecords_ReturnsZero)");
        }
        else
        {
            ValidateGetTotalBytesTests(getTotalBytesTests, problems);
        }

        // Check for test method attributes using modern API
        foreach (var method in testMethods)
        {
            var hasFactAttribute = method.GetCustomAttributes<FactAttribute>().Any();
            var hasTheoryAttribute = method.GetCustomAttributes<TheoryAttribute>().Any();

            if (!hasFactAttribute && !hasTheoryAttribute)
            {
                problems.Add($"Test method {method.Name} is missing [Fact] or [Theory] attribute");
            }

            // Check for Should() usage in test names - prefer descriptive naming
            if (method.Name.Contains("Should", StringComparison.Ordinal))
            {
                problems.Add($"Test method {method.Name} uses 'Should' in name - prefer descriptive naming without 'Should'");
            }

            // Validate method signatures for async Task patterns using pattern matching
            switch (method.Name)
            {
                case var _ when method.Name.StartsWith("Constructor_"):
                    if (method.ReturnType != typeof(void))
                    {
                        problems.Add($"Constructor test method {method.Name} should return void, found {method.ReturnType.Name}");
                    }
                    break;

                case var _ when method.Name.StartsWith("RecordUsageAsync_") ||
                             method.Name.StartsWith("GetUsageStatisticsAsync_") ||
                             method.Name.StartsWith("GetUsageRecordsAsync_") ||
                             method.Name.StartsWith("GetTotalBytesUsedAsync_"):
                    if (method.ReturnType != typeof(Task))
                    {
                        problems.Add($"Async test method {method.Name} should return Task, found {method.ReturnType.Name}");
                    }
                    break;
            }
        }

        return problems;
    }

    /// <summary>
    /// Checks if the test class instance is valid according to validation rules.
    /// </summary>
    /// <param name="value">The test class instance to check.</param>
    /// <returns>True if valid, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this UsageTrackingServiceTests value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Validates the test class instance and throws if invalid.
    /// </summary>
    /// <param name="value">The test class instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails with list of problems.</exception>
    public static void EnsureValid(this UsageTrackingServiceTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join(Environment.NewLine, problems), nameof(value));
        }
    }

    private static void ValidateConstructorTests(IReadOnlyList<MethodInfo> constructorTests, ICollection<string> problems)
    {
        var hasNullRepositoryTest = constructorTests.Any(m => m.Name.Contains("NullRepository"));
        var hasNullLoggerTest = constructorTests.Any(m => m.Name.Contains("NullLogger"));

        if (!hasNullRepositoryTest)
        {
            problems.Add("Missing Constructor_NullRepository_ThrowsArgumentNullException test");
        }

        if (!hasNullLoggerTest)
        {
            problems.Add("Missing Constructor_NullLogger_ThrowsArgumentNullException test");
        }
    }

    private static void ValidateRecordUsageTests(IReadOnlyList<MethodInfo> recordUsageTests, ICollection<string> problems)
    {
        var hasNullRecordTest = recordUsageTests.Any(m => m.Name.Contains("NullRecord"));
        var hasValidRecordTest = recordUsageTests.Any(m => m.Name.Contains("ValidRecord"));
        var hasRepositoryThrowsTest = recordUsageTests.Any(m => m.Name.Contains("RepositoryThrows"));

        if (!hasNullRecordTest)
        {
            problems.Add("Missing RecordUsageAsync_NullRecord_ThrowsArgumentNullException test");
        }

        if (!hasValidRecordTest)
        {
            problems.Add("Missing RecordUsageAsync_ValidRecord_CreatesInRepository test");
        }

        if (!hasRepositoryThrowsTest)
        {
            problems.Add("Missing RecordUsageAsync_RepositoryThrows_WrapsInDataAccessException test");
        }
    }

    private static void ValidateGetUsageStatisticsTests(IReadOnlyList<MethodInfo> getUsageStatsTests, ICollection<string> problems)
    {
        var hasEmptyKeyTest = getUsageStatsTests.Any(m => m.Name.Contains("EmptyOrNullKeyId"));
        var hasDateRangeTest = getUsageStatsTests.Any(m => m.Name.Contains("EndDateBeforeStartDate"));
        var hasValidRangeTest = getUsageStatsTests.Any(m => m.Name.Contains("ValidDateRange"));
        var hasNoRecordsTest = getUsageStatsTests.Any(m => m.Name.Contains("NoRecords"));

        if (!hasEmptyKeyTest)
        {
            problems.Add("Missing GetUsageStatisticsAsync_EmptyOrNullKeyId_ThrowsValidationException test");
        }

        if (!hasDateRangeTest)
        {
            problems.Add("Missing GetUsageStatisticsAsync_EndDateBeforeStartDate_ThrowsValidationException test");
        }

        if (!hasValidRangeTest)
        {
            problems.Add("Missing GetUsageStatisticsAsync_ValidDateRange_ReturnsStatisticsWithCorrectAggregates test");
        }

        if (!hasNoRecordsTest)
        {
            problems.Add("Missing GetUsageStatisticsAsync_NoRecords_ReturnsZeroedStatistics test");
        }
    }

    private static void ValidateGetUsageRecordsTests(IReadOnlyList<MethodInfo> getUsageRecordsTests, ICollection<string> problems)
    {
        var hasEmptyKeyTest = getUsageRecordsTests.Any(m => m.Name.Contains("EmptyOrNullKeyId"));
        var hasValidRangeTest = getUsageRecordsTests.Any(m => m.Name.Contains("ValidDateRange"));

        if (!hasEmptyKeyTest)
        {
            problems.Add("Missing GetUsageRecordsAsync_EmptyOrNullKeyId_ThrowsValidationException test");
        }

        if (!hasValidRangeTest)
        {
            problems.Add("Missing GetUsageRecordsAsync_ValidDateRange_ReturnsRecordsFromRepository test");
        }
    }

    private static void ValidateGetTotalBytesTests(IReadOnlyList<MethodInfo> getTotalBytesTests, ICollection<string> problems)
    {
        var hasEmptyConsumerTest = getTotalBytesTests.Any(m => m.Name.Contains("EmptyOrNullConsumerId"));
        var hasValidConsumerTest = getTotalBytesTests.Any(m => m.Name.Contains("ValidConsumerId"));
        var hasNoRecordsTest = getTotalBytesTests.Any(m => m.Name.Contains("NoRecords"));

        if (!hasEmptyConsumerTest)
        {
            problems.Add("Missing GetTotalBytesUsedAsync_EmptyOrNullConsumerId_ReturnsZero test");
        }

        if (!hasValidConsumerTest)
        {
            problems.Add("Missing GetTotalBytesUsedAsync_ValidConsumerId_ReturnsAggregatedBytes test");
        }

        if (!hasNoRecordsTest)
        {
            problems.Add("Missing GetTotalBytesUsedAsync_NoRecords_ReturnsZero test");
        }
    }
}