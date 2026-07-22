// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Unit tests for AuditLogEventHandlerValidation extension methods.
// =====================================================================
using Xunit;
using ApiKeyGateway.Events;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.Extensions.DependencyInjection;

namespace ApiKeyGateway.Tests;

/// <summary>Unit tests for AuditLogEventHandlerValidation extension methods.</summary>
public class AuditLogEventHandlerValidationTests
{
    private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
    private readonly Mock<ILogger<AuditLogEventHandler>> _loggerMock;
    private readonly AuditLogEventHandler _handler;
    private readonly Mock<IServiceScope> _scopeMock;

    public AuditLogEventHandlerValidationTests()
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

    [Fact] public void Validate_HappyPath_ReturnsEmptyErrorsList()
    {
        var result = _handler.Validate();
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact] public void Validate_NullHandler_ThrowsArgumentNullException()
    {
        AuditLogEventHandler? nullHandler = null;
        Assert.Throws<ArgumentNullException>(() => nullHandler!.Validate());
    }

    [Fact] public void Validate_MultipleCalls_ReturnsConsistentEmptyLists()
    {
        var result1 = _handler.Validate();
        var result2 = _handler.Validate();
        result1.Should().BeEmpty();
        result2.Should().BeEmpty();
    }

    [Fact] public void IsValid_HappyPath_ReturnsTrue()
    {
        var result = _handler.IsValid();
        result.Should().BeTrue();
    }

    [Fact] public void IsValid_ReturnsTrue_WhenValidateHasNoErrors()
    {
        var errors = _handler.Validate();
        var isValid = _handler.IsValid();
        isValid.Should().BeTrue();
        errors.Should().BeEmpty();
    }

    [Fact] public void IsValid_NullHandler_ThrowsArgumentNullException()
    {
        AuditLogEventHandler? nullHandler = null;
        Assert.Throws<ArgumentNullException>(() => nullHandler!.IsValid());
    }

    [Fact] public void IsValid_ConsistentWithValidate()
    {
        var errors = _handler.Validate();
        var isValid = _handler.IsValid();
        isValid.Should().Be(errors.Count == 0);
    }

    [Fact] public void EnsureValid_HappyPath_NoExceptionThrown()
    {
        var act = () => _handler.EnsureValid();
        act.Should().NotThrow();
    }

    [Fact] public void EnsureValid_NullHandler_ThrowsArgumentNullException()
    {
        AuditLogEventHandler? nullHandler = null;
        Assert.Throws<ArgumentNullException>(() => nullHandler!.EnsureValid());
    }

    [Fact] public void EnsureValid_Throws_WhenHandlerIsInvalid()
    {
        AuditLogEventHandler? nullHandler = null;
        var act = () => nullHandler!.EnsureValid();
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact] public void EnsureValid_MultipleCalls_NoExceptionThrown()
    {
        var act1 = () => _handler.EnsureValid();
        var act2 = () => _handler.EnsureValid();
        act1.Should().NotThrow();
        act2.Should().NotThrow();
    }
}
