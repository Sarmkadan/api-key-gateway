using System;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using ApiKeyGateway.Domain.Models;

namespace ApiKeyGateway.Tests;

public class TransformationRuleTests
{
    [Fact]
    public void DefaultConstructor_ShouldInitializeWithExpectedDefaults()
    {
        // Arrange
        var rule = new TransformationRule();

        // Act & Assert
        rule.Id.Should().Be(string.Empty);
        rule.Name.Should().Be(string.Empty);
        rule.Description.Should().BeNull();
        rule.Scope.Should().Be(TransformationScope.Global);
        rule.ApiKeyId.Should().BeNull();
        rule.ConsumerId.Should().BeNull();
        rule.Type.Should().Be(TransformationRuleType.BuiltIn);
        rule.Action.Should().BeNull();
        rule.LuaScript.Should().BeNull();
        rule.Parameters.Should().NotBeNull();
        rule.Parameters.Should().BeEmpty();
        rule.Priority.Should().Be(100);
        rule.IsEnabled.Should().BeTrue();
        rule.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        rule.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        rule.CreatedBy.Should().BeNull();
    }

    [Fact]
    public void PropertySetters_ShouldPersistValues()
    {
        // Arrange
        var rule = new TransformationRule
        {
            Id = "rule-123",
            Name = "Add Header",
            Description = "Adds X-Test header",
            Scope = TransformationScope.ApiKey,
            ApiKeyId = "key-456",
            ConsumerId = "consumer-789",
            Type = TransformationRuleType.LuaScript,
            Action = BuiltInAction.AddHeader,
            LuaScript = "return true",
            Priority = 10,
            IsEnabled = false,
            CreatedBy = "admin"
        };

        // Act & Assert
        rule.Id.Should().Be("rule-123");
        rule.Name.Should().Be("Add Header");
        rule.Description.Should().Be("Adds X-Test header");
        rule.Scope.Should().Be(TransformationScope.ApiKey);
        rule.ApiKeyId.Should().Be("key-456");
        rule.ConsumerId.Should().Be("consumer-789");
        rule.Type.Should().Be(TransformationRuleType.LuaScript);
        rule.Action.Should().Be(BuiltInAction.AddHeader);
        rule.LuaScript.Should().Be("return true");
        rule.Priority.Should().Be(10);
        rule.IsEnabled.Should().BeFalse();
        rule.CreatedBy.Should().Be("admin");
    }

    [Fact]
    public void ParametersDictionary_ShouldAllowMutations()
    {
        // Arrange
        var rule = new TransformationRule();

        // Act
        rule.Parameters["HeaderName"] = "X-Test";
        rule.Parameters["HeaderValue"] = "value";

        // Assert
        rule.Parameters.Should().ContainKey("HeaderName");
        rule.Parameters.Should().ContainKey("HeaderValue");
        rule.Parameters["HeaderName"].Should().Be("X-Test");
        rule.Parameters["HeaderValue"].Should().Be("value");
        rule.Parameters.Count.Should().Be(2);
    }

    [Fact]
    public void From_ShouldMapEntityToDtoCorrectly()
    {
        // Arrange
        var rule = new TransformationRule
        {
            Id = "id-1",
            Name = "Rule",
            Description = "Desc",
            Scope = TransformationScope.Consumer,
            ApiKeyId = null,
            ConsumerId = "consumer-1",
            Type = TransformationRuleType.BuiltIn,
            Action = BuiltInAction.RemoveHeader,
            LuaScript = null,
            Parameters = new Dictionary<string, string> { { "Key", "Value" } },
            Priority = 20,
            IsEnabled = true,
            CreatedAt = new DateTime(2023, 1, 1),
            UpdatedAt = new DateTime(2023, 1, 2),
            CreatedBy = "creator"
        };

        // Act
        var dto = TransformationRuleDto.From(rule);

        // Assert
        dto.Id.Should().Be(rule.Id);
        dto.Name.Should().Be(rule.Name);
        dto.Description.Should().Be(rule.Description);
        dto.Scope.Should().Be(rule.Scope);
        dto.ApiKeyId.Should().Be(rule.ApiKeyId);
        dto.ConsumerId.Should().Be(rule.ConsumerId);
        dto.Type.Should().Be(rule.Type);
        dto.Action.Should().Be(rule.Action);
        dto.Parameters.Should().BeEquivalentTo(rule.Parameters);
        dto.Priority.Should().Be(rule.Priority);
        dto.IsEnabled.Should().Be(rule.IsEnabled);
        dto.CreatedAt.Should().Be(rule.CreatedAt);
        dto.UpdatedAt.Should().Be(rule.UpdatedAt);
    }

    [Fact]
    public void SettingNullValues_ShouldNotThrow()
    {
        // Arrange
        var rule = new TransformationRule();

        // Act
        var act = () =>
        {
            rule.Description = null;
            rule.ApiKeyId = null;
            rule.ConsumerId = null;
            rule.LuaScript = null;
            rule.Action = null;
        };

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EmptyStrings_ShouldBeAccepted()
    {
        // Arrange
        var rule = new TransformationRule
        {
            Id = string.Empty,
            Name = string.Empty
        };

        // Act & Assert
        rule.Id.Should().Be(string.Empty);
        rule.Name.Should().Be(string.Empty);
    }
}
