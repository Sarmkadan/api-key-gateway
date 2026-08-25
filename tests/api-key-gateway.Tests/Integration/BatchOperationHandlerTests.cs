using ApiKeyGateway.Domain.Exceptions;
using ApiKeyGateway.Integration;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ApiKeyGateway.Tests.Integration;

/// <summary>
/// Test class for <see cref="BatchOperationHandler"/>.
/// </summary>
public class BatchOperationHandlerTests
{
    private readonly Mock<ILogger<BatchOperationHandler>> _loggerMock;
    private readonly BatchOperationHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="BatchOperationHandlerTests"/> class.
    /// Sets up the mock logger and the handler under test.
    /// </summary>
    public BatchOperationHandlerTests()
    {
        _loggerMock = new Mock<ILogger<BatchOperationHandler>>();
        _handler = new BatchOperationHandler(_loggerMock.Object);
    }

    /// <summary>
    /// Verifies that executing an empty batch operation throws a validation exception.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_EmptyBatch_ThrowsValidationException()
    {
        _loggerMock.Object.LogInformation("Starting test {TestName}", nameof(ExecuteAsync_EmptyBatch_ThrowsValidationException));
        // Arrange
        var operation = new BatchOperation
        {
            OperationType = "disable",
            ApiKeyIds = new List<string>()
        };

        // Act & Assert
        var act = () => _handler.ExecuteAsync(operation);
        await act.Should().ThrowAsync<ValidationException>(
            "At least one API key ID must be specified");
    }

    /// <summary>
    /// Verifies that executing a null operation throws an argument null exception.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_NullOperation_ThrowsArgumentNullException()
    {
        // Arrange
        BatchOperation operation = null!;

        // Act & Assert
        var act = () => _handler.ExecuteAsync(operation);
        await act.Should().ThrowAsync<ArgumentNullException>(
            "Value cannot be null. (Parameter 'operation')");
    }

    /// <summary>
    /// Verifies that a single item batch is processed successfully.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_SingleItemBatch_ProcessesSuccessfully()
    {
        _loggerMock.Object.LogInformation("Starting test {TestName}", nameof(ExecuteAsync_SingleItemBatch_ProcessesSuccessfully));
        // Arrange
        var operation = new BatchOperation
        {
            OperationType = "disable",
            ApiKeyIds = new List<string> { "key-123" }
        };

        // Act
        var result = await _handler.ExecuteAsync(operation);

        // Assert
        result.Should().NotBeNull();
        result.OperationId.Should().Be(operation.Id);
        result.TotalCount.Should().Be(1);
        result.SuccessCount.Should().Be(1);
        result.FailureCount.Should().Be(0);
        result.Items.Should().HaveCount(1);
        result.Items[0].ApiKeyId.Should().Be("key-123");
        result.Items[0].Success.Should().BeTrue();
        result.Items[0].ErrorMessage.Should().BeNull();
        result.Items[0].Result.Should().Be("Key disabled");
    }

    /// <summary>
    /// Verifies that a multiple item batch is processed successfully.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_MultipleItemsBatch_ProcessesAllSuccessfully()
    {
        _loggerMock.Object.LogInformation("Starting test {TestName}", nameof(ExecuteAsync_MultipleItemsBatch_ProcessesAllSuccessfully));
        // Arrange
        var operation = new BatchOperation
        {
            OperationType = "enable",
            ApiKeyIds = new List<string> { "key-1", "key-2", "key-3" }
        };

        // Act
        var result = await _handler.ExecuteAsync(operation);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(3);
        result.SuccessCount.Should().Be(3);
        result.FailureCount.Should().Be(0);
        result.Items.Should().HaveCount(3);

        foreach (var item in result.Items)
        {
            item.Success.Should().BeTrue();
            item.ErrorMessage.Should().BeNull();
            item.Result.Should().Be("Key enabled");
        }
    }

    /// <summary>
    /// Verifies that a batch larger than the chunk size is processed completely.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_BatchLargerThanChunkSize_ProcessesAllItems()
    {
        _loggerMock.Object.LogInformation("Starting test {TestName}", nameof(ExecuteAsync_BatchLargerThanChunkSize_ProcessesAllItems));
        // Arrange - Create a batch larger than typical chunk sizes
        var largeBatch = new List<string>();
        for (int i = 0; i < 100; i++)
        {
            largeBatch.Add($"key-{i:D3}");
        }

        var operation = new BatchOperation
        {
            OperationType = "rotate",
            ApiKeyIds = largeBatch
        };

        // Act
        var result = await _handler.ExecuteAsync(operation);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(100);
        result.SuccessCount.Should().Be(100);
        result.FailureCount.Should().Be(0);
        result.Items.Should().HaveCount(100);

        foreach (var item in result.Items)
        {
            item.Success.Should().BeTrue();
            item.ErrorMessage.Should().BeNull();
            item.Result.Should().Be("Key rotated");
        }
    }

    /// <summary>
    /// Verifies that a set-quota operation with parameters is processed successfully.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_SetQuotaOperation_ProcessesWithParameters()
    {
        // Arrange
        var operation = new BatchOperation
        {
            OperationType = "set-quota",
            ApiKeyIds = new List<string> { "key-quota-1", "key-quota-2" },
            Parameters = new Dictionary<string, object> { { "quotaLimit", 1000 } }
        };

        // Act
        var result = await _handler.ExecuteAsync(operation);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2);
        result.SuccessCount.Should().Be(2);
        result.FailureCount.Should().Be(0);
        result.Items.Should().HaveCount(2);

        foreach (var item in result.Items)
        {
            item.Success.Should().BeTrue();
            item.ErrorMessage.Should().BeNull();
            item.Result.Should().Be("Quota set to 1000");
        }
    }

    /// <summary>
    /// Verifies that a set-quota operation without parameters results in failure for all items.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_SetQuotaWithoutParameters_ReturnsFailureForAllItems()
    {
        // Arrange
        var operation = new BatchOperation
        {
            OperationType = "set-quota",
            ApiKeyIds = new List<string> { "key-no-param-1", "key-no-param-2" }
        };

        // Act
        var result = await _handler.ExecuteAsync(operation);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2);
        result.SuccessCount.Should().Be(0);
        result.FailureCount.Should().Be(2);
        result.Items.Should().HaveCount(2);

        foreach (var item in result.Items)
        {
            item.Success.Should().BeFalse();
            item.ErrorMessage.Should().Be("Missing or invalid quotaLimit parameter");
            item.Result.Should().BeNull();
        }
    }

    /// <summary>
    /// Verifies that a set-quota operation with an invalid quota parameter results in failure for all items.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_InvalidQuotaParameter_ReturnsFailureForAllItems()
    {
        // Arrange
        var operation = new BatchOperation
        {
            OperationType = "set-quota",
            ApiKeyIds = new List<string> { "key-invalid-1", "key-invalid-2" },
            Parameters = new Dictionary<string, object> { { "quotaLimit", "not-a-number" } }
        };

        // Act
        var result = await _handler.ExecuteAsync(operation);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2);
        result.SuccessCount.Should().Be(0);
        result.FailureCount.Should().Be(2);
        result.Items.Should().HaveCount(2);

        foreach (var item in result.Items)
        {
            item.Success.Should().BeFalse();
            item.ErrorMessage.Should().Be("Missing or invalid quotaLimit parameter");
            item.Result.Should().BeNull();
        }
    }

    /// <summary>
    /// Verifies that an unknown operation type results in failure for all items.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_UnknownOperationType_ReturnsFailureForAllItems()
    {
        // Arrange
        var operation = new BatchOperation
        {
            OperationType = "unknown-operation",
            ApiKeyIds = new List<string> { "key-unknown-1", "key-unknown-2" }
        };

        // Act
        var result = await _handler.ExecuteAsync(operation);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2);
        result.SuccessCount.Should().Be(0);
        result.FailureCount.Should().Be(2);
        result.Items.Should().HaveCount(2);

        foreach (var item in result.Items)
        {
            item.Success.Should().BeFalse();
            item.ErrorMessage.Should().Be("Unknown operation type: unknown-operation");
            item.Result.Should().BeNull();
        }
    }

    /// <summary>
    /// Verifies that a batch with mixed success and failure items reports correct counts.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_MixedSuccessAndFailureItems_ReportsCorrectCounts()
    {
        // Arrange - Mix valid and invalid operations
        var operation = new BatchOperation
        {
            OperationType = "set-quota",
            ApiKeyIds = new List<string> { "key-valid-1", "key-invalid-1", "key-valid-2", "key-invalid-2" },
            Parameters = new Dictionary<string, object> { { "quotaLimit", 500 } }
        };

        // Act
        var result = await _handler.ExecuteAsync(operation);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(4);
        result.SuccessCount.Should().Be(2);
        result.FailureCount.Should().Be(2);
        result.Items.Should().HaveCount(4);

        // Check that valid items succeeded
        result.Items.Count(x => x.ApiKeyId.StartsWith("key-valid") && x.Success).Should().Be(2);

        // Check that invalid items failed
        result.Items.Count(x => x.ApiKeyId.StartsWith("key-invalid") && !x.Success).Should().Be(2);
    }

    /// <summary>
    /// Verifies that when an exception occurs in a single item, processing continues and the error is reported.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_ExceptionInSingleItem_ContinuesProcessingAndReportsError()
    {
        // Arrange - Create a mock that will throw for specific keys by wrapping the handler
        var mockLogger = new Mock<ILogger<BatchOperationHandler>>();
        var handler = new BatchOperationHandler(mockLogger.Object);
        var operation = new BatchOperation
        {
            OperationType = "disable",
            ApiKeyIds = new List<string> { "key-normal-1", "key-error-1", "key-normal-2" }
        };

        // Act
        var result = await handler.ExecuteAsync(operation);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(3);
        result.SuccessCount.Should().Be(2);
        result.FailureCount.Should().Be(1);
        result.Items.Should().HaveCount(3);

        // First item should succeed
        result.Items[0].ApiKeyId.Should().Be("key-normal-1");
        result.Items[0].Success.Should().BeTrue();

        // Second item should fail with exception message
        result.Items[1].ApiKeyId.Should().Be("key-error-1");
        result.Items[1].Success.Should().BeFalse();
        result.Items[1].ErrorMessage.Should().Contain("Simulated error");

        // Third item should succeed
        result.Items[2].ApiKeyId.Should().Be("key-normal-2");
        result.Items[2].Success.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that when all items fail, all failures are reported.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_AllItemsFail_ReportsAllFailures()
    {
        // Arrange
        var operation = new BatchOperation
        {
            OperationType = "disable",
            ApiKeyIds = new List<string> { "key-error-1", "key-error-2", "key-error-3" }
        };

        // Act
        var result = await _handler.ExecuteAsync(operation);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(3);
        result.SuccessCount.Should().Be(0);
        result.FailureCount.Should().Be(3);
        result.Items.Should().HaveCount(3);

        foreach (var item in result.Items)
        {
            item.Success.Should().BeFalse();
            item.ErrorMessage.Should().NotBeNullOrEmpty();
        }
    }

    /// <summary>
    /// Verifies that the result properties (like OperationId, CompletedAt) are correctly set.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_ResultPropertiesAreCorrectlySet()
    {
        // Arrange
        var startTime = DateTime.UtcNow.AddMinutes(-1);
        var operation = new BatchOperation
        {
            OperationType = "rotate",
            ApiKeyIds = new List<string> { "key-1", "key-2", "key-3" }
        };

        // Act
        var result = await _handler.ExecuteAsync(operation);
        var endTime = DateTime.UtcNow.AddMinutes(1);

        // Assert
        result.OperationId.Should().Be(operation.Id);
        result.TotalCount.Should().Be(3);
        result.SuccessCount.Should().Be(3);
        result.FailureCount.Should().Be(0);
        result.CompletedAt.Should().BeAfter(startTime);
        result.CompletedAt.Should().BeBefore(endTime);
    }

    /// <summary>
    /// Verifies that the item results contain the correct API key IDs.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_ItemResultsContainCorrectApiKeyIds()
    {
        // Arrange
        var apiKeyIds = new List<string> { "key-a", "key-b", "key-c", "key-d", "key-e" };
        var operation = new BatchOperation
        {
            OperationType = "disable",
            ApiKeyIds = apiKeyIds
        };

        // Act
        var result = await _handler.ExecuteAsync(operation);

        // Assert
        result.Items.Select(x => x.ApiKeyId).Should().BeEquivalentTo(apiKeyIds);
    }
}