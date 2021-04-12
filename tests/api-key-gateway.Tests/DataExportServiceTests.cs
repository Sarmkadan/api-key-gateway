using Xunit;
using ApiKeyGateway.Services;
using ApiKeyGateway.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Contains unit tests for the DataExportService class, verifying correct behavior of API key export functionality.
/// </summary>
public class DataExportServiceTests
{
    /// <summary>
    /// Mock repository for API key data access.
    /// </summary>
    private readonly Mock<IApiKeyRepository> _apiKeyRepositoryMock;

    /// <summary>
    /// Mock repository for audit log data access.
    /// </summary>
    private readonly Mock<IAuditLogRepository> _auditLogRepositoryMock;

    /// <summary>
    /// Mock repository for usage data access.
    /// </summary>
    private readonly Mock<IUsageRepository> _usageRepositoryMock;

    /// <summary>
    /// Mock logger for DataExportService.
    /// </summary>
    private readonly Mock<ILogger<DataExportService>> _loggerMock;

    /// <summary>
    /// System under test (DataExportService instance) configured with mocked dependencies.
    /// </summary>
    private readonly DataExportService _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataExportServiceTests"/> class.
    /// Sets up all required mocks and the system under test.
    /// </summary>
    public DataExportServiceTests()
    {
        _apiKeyRepositoryMock = new Mock<IApiKeyRepository>();
        _auditLogRepositoryMock = new Mock<IAuditLogRepository>();
        _usageRepositoryMock = new Mock<IUsageRepository>();
        _loggerMock = new Mock<ILogger<DataExportService>>();
        
        _sut = new DataExportService(
            _apiKeyRepositoryMock.Object,
            _auditLogRepositoryMock.Object,
            _usageRepositoryMock.Object,
            _loggerMock.Object);
    }

    /// <summary>
    /// Verifies that ExportApiKeysAsync returns non-null data when provided with a valid format.
    /// </summary>
    [Fact]
    public async Task ExportApiKeysAsync_ValidFormat_ReturnsData()
    {
        // Arrange
        _apiKeyRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Domain.Models.ApiKey>());

        // Act
        var result = await _sut.ExportApiKeysAsync("json");

        // Assert
        result.Should().NotBeNull();
    }

    /// <summary>
    /// Verifies that ExportApiKeysAsync throws DataAccessException when the repository throws an exception.
    /// </summary>
    [Fact]
    public async Task ExportApiKeysAsync_RepositoryThrows_ThrowsException()
    {
        // Arrange
        _apiKeyRepositoryMock.Setup(repo => repo.GetAllAsync()).ThrowsAsync(new Exception("DB Error"));

        // Act
        var act = async () => await _sut.ExportApiKeysAsync("json");

        // Assert
        (await act.Should().ThrowAsync<ApiKeyGateway.Domain.Exceptions.DataAccessException>())
            .WithInnerException<Exception>()
            .WithMessage("DB Error");
    }
}
