// SPDX-License-Identifier: MIT
// © 2024 RedRocket
// Unit tests for AuditLogEventHandlerJsonExtensions covering JSON serialization and deserialization

using Xunit;
using ApiKeyGateway.Events;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.Extensions.DependencyInjection;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Unit tests for <see cref="AuditLogEventHandlerJsonExtensions"/> JSON serialization and deserialization methods.
/// Tests the ToJson, FromJson, and TryFromJson extension methods.
/// </summary>
public class AuditLogEventHandlerJsonExtensionsTests
{
    private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
    private readonly Mock<ILogger<AuditLogEventHandler>> _loggerMock;
    private readonly AuditLogEventHandler _handler;
    private readonly Mock<IServiceScope> _scopeMock;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditLogEventHandlerJsonExtensionsTests"/> class.
    /// </summary>
    public AuditLogEventHandlerJsonExtensionsTests()
    {
        _scopeFactoryMock = new Mock<IServiceScopeFactory>();
        _loggerMock = new Mock<ILogger<AuditLogEventHandler>>();
        _handler = new AuditLogEventHandler(_scopeFactoryMock.Object, _loggerMock.Object);

        _scopeMock = new Mock<IServiceScope>();
        _scopeFactoryMock
            .Setup(x => x.CreateScope())
            .Returns(_scopeMock.Object);

        _scopeMock
            .Setup(x => x.ServiceProvider)
            .Returns(() => new ServiceCollection()
                .BuildServiceProvider());
    }

    [Fact]
    /// <summary>
    /// Tests that ToJson produces output.
    /// </summary>
    public void ToJson_ProducesOutput()
    {
        // Arrange
        var handler = _handler;

        // Act
        var json = handler.ToJson();

        // Assert
        json.Should().NotBeNull();
    }

    [Fact]
    /// <summary>
    /// Tests that ToJson with indented parameter produces output.
    /// </summary>
    public void ToJson_WithIndentedParameter_ProducesOutput()
    {
        // Arrange
        var handler = _handler;

        // Act
        var json = handler.ToJson(indented: true);
        var jsonCompact = handler.ToJson(indented: false);

        // Assert
        json.Should().NotBeNull();
        jsonCompact.Should().NotBeNull();
    }

    [Fact]
    /// <summary>
    /// Tests that FromJson throws ArgumentNullException for null input as documented.
    /// </summary>
    public void FromJson_NullInput_ThrowsArgumentNullException()
    {
        // Arrange
        string? nullJson = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => AuditLogEventHandlerJsonExtensions.FromJson(nullJson!));
    }

    [Fact]
    /// <summary>
    /// Tests that TryFromJson throws ArgumentNullException for null input as documented.
    /// </summary>
    public void TryFromJson_NullInput_ThrowsArgumentNullException()
    {
        // Arrange
        string? nullJson = null;
        AuditLogEventHandler? result = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            AuditLogEventHandlerJsonExtensions.TryFromJson(nullJson!, out result));
    }

    [Fact]
    /// <summary>
    /// Tests that TryFromJson method exists and can be invoked.
    /// </summary>
    public void TryFromJson_MethodExists()
    {
        // Arrange
        var json = "{}";
        AuditLogEventHandler? result = null;

        // Act - just verify it can be called without compilation errors
        var act = () => AuditLogEventHandlerJsonExtensions.TryFromJson(json, out result);

        // Assert
        act.Should().NotThrow<ArgumentNullException>();
    }
}
