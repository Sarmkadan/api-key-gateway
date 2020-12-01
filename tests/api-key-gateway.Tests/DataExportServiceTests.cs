using Xunit;
using ApiKeyGateway.Services;
using ApiKeyGateway.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApiKeyGateway.Tests;

public class DataExportServiceTests
{
    private readonly Mock<IApiKeyRepository> _apiKeyRepositoryMock;
    private readonly Mock<IAuditLogRepository> _auditLogRepositoryMock;
    private readonly Mock<IUsageRepository> _usageRepositoryMock;
    private readonly Mock<ILogger<DataExportService>> _loggerMock;
    private readonly DataExportService _sut;

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

    [Fact]
    public async Task ExportApiKeysAsync_RepositoryThrows_ThrowsException()
    {
        // Arrange
        _apiKeyRepositoryMock.Setup(repo => repo.GetAllAsync()).ThrowsAsync(new Exception("DB Error"));

        // Act
        var act = async () => await _sut.ExportApiKeysAsync("json");

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("DB Error");
    }
}
