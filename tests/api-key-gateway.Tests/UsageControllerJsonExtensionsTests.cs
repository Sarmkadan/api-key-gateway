using System;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Reflection;
using ApiKeyGateway.Controllers;
using Xunit;

namespace api_key_gateway.Tests
{
    public class UsageControllerJsonExtensionsTests
    {
        // Helper to create an uninitialized instance of UsageController (avoids constructor requirements)
        private static UsageController CreateEmptyUsageController()
        {
            return (UsageController)FormatterServices.GetUninitializedObject(typeof(UsageController));
        }

        [Fact]
        public void ToJson_WithValidInstance_ReturnsJsonString()
        {
            // Arrange
            var controller = CreateEmptyUsageController();

            // Act
            var json = controller.ToJson();

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(json));
            Assert.StartsWith("{", json);
            Assert.EndsWith("}", json);
        }

        [Fact]
        public void ToJson_WithIndentation_ReturnsPrettyJson()
        {
            var controller = CreateEmptyUsageController();

            var json = controller.ToJson(indented: true);

            Assert.Contains("\n", json); // pretty printed JSON contains line breaks
        }

        [Fact]
        public void ToJson_NullInstance_ThrowsArgumentNullException()
        {
            UsageController? nullController = null;
            Assert.Throws<ArgumentNullException>(() => nullController!.ToJson());
        }

        [Fact]
        public void FromJson_WithValidJson_ReturnsInstance()
        {
            // Minimal valid JSON for a controller – empty object works for most DTO‑like controllers
            var json = "{}";

            var result = UsageControllerJsonExtensions.FromJson(json);

            Assert.NotNull(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void FromJson_NullOrEmpty_ThrowsArgumentException(string json)
        {
            Assert.Throws<ArgumentException>(() => UsageControllerJsonExtensions.FromJson(json));
        }

        [Fact]
        public void TryFromJson_WithValidJson_ReturnsTrueAndInstance()
        {
            var json = "{}";

            var success = UsageControllerJsonExtensions.TryFromJson(json, out var result);

            Assert.True(success);
            Assert.NotNull(result);
        }

        [Fact]
        public void TryFromJson_WithInvalidJson_ReturnsFalse()
        {
            var json = "{ invalid json }";

            var success = UsageControllerJsonExtensions.TryFromJson(json, out var result);

            Assert.False(success);
            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void TryFromJson_NullOrWhiteSpace_ReturnsFalse(string json)
        {
            var success = UsageControllerJsonExtensions.TryFromJson(json, out var result);

            Assert.False(success);
            Assert.Null(result);
        }

        [Fact]
        public void JsonContextModifier_ApplyCamelCaseNaming_ConvertsPropertyNames()
        {
            // Arrange: a simple test model with a PascalCase property
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            };
            var typeInfo = options.GetTypeInfo(typeof(TestModel));

            // Act: invoke the private modifier via reflection
            var modifierType = typeof(UsageControllerJsonExtensions)
                .GetNestedType("JsonContextModifier", BindingFlags.NonPublic);
            var method = modifierType!.GetMethod("ApplyCamelCaseNaming", BindingFlags.NonPublic | BindingFlags.Static);
            method!.Invoke(null, new object[] { typeInfo });

            // Assert: the property name should now be camelCase
            var property = Assert.Single(typeInfo.Properties);
            Assert.Equal("pascalCaseProperty", property.Name);
        }

        // Helper class used only for the JsonContextModifier test
        private class TestModel
        {
            public string PascalCaseProperty { get; set; } = "value";
        }
    }
}
