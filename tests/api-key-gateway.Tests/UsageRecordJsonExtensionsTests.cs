// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Text.Json;
using ApiKeyGateway.Domain.Models;
using FluentAssertions;
using Xunit;

namespace ApiKeyGateway.Tests
{
    public class UsageRecordJsonExtensionsTests
    {
        [Fact]
        public void ToJson_SerializesUsageRecordCorrectly()
        {
            // Arrange
            var usageRecord = new UsageRecord
            {
                Id = "test-id",
                ApiKeyId = "test-api-key",
                ConsumerId = "test-consumer",
                RecordedAt = new DateTime(2026, 8, 18, 10, 30, 0, DateTimeKind.Utc),
                Endpoint = "/test",
                Method = "POST",
                ResponseStatusCode = 200,
                RequestBytes = 100,
                ResponseBytes = 200,
                ResponseTimeMs = 50,
                ErrorCode = "TEST_ERROR",
                SourceIp = "127.0.0.1",
                UserAgent = "test-agent",
                Tags = new Dictionary<string, string> { { "env", "test" } }
            };

            // Act
            var json = usageRecord.ToJson();

            // Assert
            json.Should().NotBeNullOrEmpty();
            json.Should().Contain("\"id\":\"test-id\"");
            json.Should().Contain("\"apiKeyId\":\"test-api-key\"");
            json.Should().Contain("\"consumerId\":\"test-consumer\"");
            json.Should().Contain("\"endpoint\":\"/test\"");
            json.Should().Contain("\"method\":\"POST\"");
            json.Should().Contain("\"responseStatusCode\":200");
            json.Should().Contain("\"requestBytes\":100");
            json.Should().Contain("\"responseBytes\":200");
            json.Should().Contain("\"responseTimeMs\":50");
            json.Should().Contain("\"errorCode\":\"TEST_ERROR\"");
            json.Should().Contain("\"sourceIp\":\"127.0.0.1\"");
            json.Should().Contain("\"userAgent\":\"test-agent\"");
            json.Should().Contain("\"tags\":{\"env\":\"test\"}");
        }

        [Fact]
        public void ToJson_ThrowsArgumentNullException_WhenUsageRecordIsNull()
        {
            // Arrange
            UsageRecord usageRecord = null;

            // Act
            Action act = () => usageRecord.ToJson();

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("value");
        }

        [Fact]
        public void ToJson_WithIndentedTrue_ProducesIndentedJson()
        {
            // Arrange
            var usageRecord = new UsageRecord
            {
                Id = "test-id",
                ApiKeyId = "test-api-key"
            };

            // Act
            var json = usageRecord.ToJson(indented: true);

            // Assert
            json.Should().Contain("{");
            json.Should().Contain("}");
            json.Should().MatchRegex(@"^\s*\{");
        }

        [Fact]
        public void FromJson_DeserializesUsageRecordCorrectly()
        {
            // Arrange
            var json = @"{
                ""id"": ""test-id"",
                ""apiKeyId"": ""test-api-key"",
                ""consumerId"": ""test-consumer"",
                ""recordedAt"": ""2026-08-18T10:30:00Z"",
                ""endpoint"": ""/test"",
                ""method"": ""POST"",
                ""responseStatusCode"": 200,
                ""requestBytes"": 100,
                ""responseBytes"": 200,
                ""responseTimeMs"": 50,
                ""errorCode"": ""TEST_ERROR"",
                ""sourceIp"": ""127.0.0.1"",
                ""userAgent"": ""test-agent"",
                ""tags"": { ""env"": ""test"" }
            }";

            // Act
            var result = UsageRecordJsonExtensions.FromJson(json);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be("test-id");
            result.ApiKeyId.Should().Be("test-api-key");
            result.ConsumerId.Should().Be("test-consumer");
            result.RecordedAt.Should().Be(new DateTime(2026, 8, 18, 10, 30, 0, DateTimeKind.Utc));
            result.Endpoint.Should().Be("/test");
            result.Method.Should().Be("POST");
            result.ResponseStatusCode.Should().Be(200);
            result.RequestBytes.Should().Be(100);
            result.ResponseBytes.Should().Be(200);
            result.ResponseTimeMs.Should().Be(50);
            result.ErrorCode.Should().Be("TEST_ERROR");
            result.SourceIp.Should().Be("127.0.0.1");
            result.UserAgent.Should().Be("test-agent");
            result.Tags.Should().ContainKey("env");
            result.Tags["env"].Should().Be("test");
        }

        [Fact]
        public void FromJson_ReturnsNull_WhenJsonIsNull()
        {
            // Arrange
            string json = null;

            // Act
            Action act = () => UsageRecordJsonExtensions.FromJson(json);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("json");
        }

        [Fact]
        public void FromJson_ReturnsNull_WhenJsonIsEmpty()
        {
            // Arrange
            var json = string.Empty;

            // Act
            var result = UsageRecordJsonExtensions.FromJson(json);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void FromJson_ReturnsNull_WhenJsonIsWhitespace()
        {
            // Arrange
            var json = "   ";

            // Act
            var result = UsageRecordJsonExtensions.FromJson(json);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void FromJson_ThrowsJsonException_WhenJsonIsInvalid()
        {
            // Arrange
            var json = @"{ invalid json }";

            // Act
            Action act = () => UsageRecordJsonExtensions.FromJson(json);

            // Assert
            act.Should().Throw<JsonException>();
        }

        [Fact]
        public void TryFromJson_ReturnsTrue_WhenJsonIsValid()
        {
            // Arrange
            var json = @"{ ""id"": ""test-id"", ""apiKeyId"": ""test-api-key"" }";

            // Act
            var result = UsageRecordJsonExtensions.TryFromJson(json, out var usageRecord);

            // Assert
            result.Should().BeTrue();
            usageRecord.Should().NotBeNull();
            usageRecord!.Id.Should().Be("test-id");
            usageRecord.ApiKeyId.Should().Be("test-api-key");
        }

        [Fact]
        public void TryFromJson_ReturnsFalse_WhenJsonIsInvalid()
        {
            // Arrange
            var json = @"{ invalid json }";

            // Act
            var result = UsageRecordJsonExtensions.TryFromJson(json, out var usageRecord);

            // Assert
            result.Should().BeFalse();
            usageRecord.Should().BeNull();
        }

        [Fact]
        public void TryFromJson_ReturnsTrue_WhenJsonIsEmpty()
        {
            // Arrange
            var json = string.Empty;

            // Act
            var result = UsageRecordJsonExtensions.TryFromJson(json, out var usageRecord);

            // Assert
            result.Should().BeTrue();
            usageRecord.Should().BeNull();
        }

        [Fact]
        public void TryFromJson_ThrowsArgumentNullException_WhenJsonIsNull()
        {
            // Arrange
            string json = null;

            // Act
            Action act = () => UsageRecordJsonExtensions.TryFromJson(json, out _);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("json");
        }
    }
}