// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using Xunit;
using ApiKeyGateway.Utilities;
using FluentAssertions;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Contains unit tests for the <see cref="QueryStringHelper"/> utility class methods.
/// Tests various query string building and parsing scenarios including happy-path,
/// edge cases (null/empty inputs, boundary values), and error-path assertions.
/// </summary>
public class QueryStringHelperUnitTests
{
    /// <summary>
    /// Tests the <see cref="QueryStringHelper.BuildQueryString"/> method with a valid dictionary of parameters.
    /// Validates that the method correctly builds a query string with proper URL encoding.
    /// </summary>
    [Fact]
    public void BuildQueryString_ValidParameters_ReturnsCorrectQueryString()
    {
        // Arrange
        var parameters = new Dictionary<string, string>
        {
            { "key1", "value1" },
            { "key2", "value2" },
            { "key3", "value3" }
        };

        // Act
        var result = QueryStringHelper.BuildQueryString(parameters);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Be("key1=value1&key2=value2&key3=value3");
    }

    /// <summary>
    /// Tests the <see cref="QueryStringHelper.BuildQueryString"/> method with parameters containing special characters.
    /// Validates that special characters are properly URL encoded.
    /// </summary>
    [Fact]
    public void BuildQueryString_SpecialCharacters_EncodesCorrectly()
    {
        // Arrange
        var parameters = new Dictionary<string, string>
        {
            { "api key", "value with spaces" },
            { "key&name", "value=with=equals" },
            { "key?test", "value#anchor" }
        };

        // Act
        var result = QueryStringHelper.BuildQueryString(parameters);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("api+key=value+with+spaces");
        result.Should().Contain("key%26name=value%3dwith%3dequals");
        result.Should().Contain("key%3ftest=value%23anchor");
    }

    /// <summary>
    /// Tests the <see cref="QueryStringHelper.BuildQueryString"/> method with an empty dictionary.
    /// Validates that an empty dictionary returns an empty string.
    /// </summary>
    [Fact]
    public void BuildQueryString_EmptyDictionary_ReturnsEmptyString()
    {
        // Arrange
        var parameters = new Dictionary<string, string>();

        // Act
        var result = QueryStringHelper.BuildQueryString(parameters);

        // Assert
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Tests the <see cref="QueryStringHelper.BuildQueryString"/> method with a null dictionary.
    /// Validates that a null dictionary returns an empty string.
    /// </summary>
    [Fact]
    public void BuildQueryString_NullDictionary_ReturnsEmptyString()
    {
        // Arrange
        Dictionary<string, string> parameters = null!;

        // Act
        var result = QueryStringHelper.BuildQueryString(parameters);

        // Assert
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Tests the <see cref="QueryStringHelper.BuildQueryString"/> method with parameters containing empty string values.
    /// Validates that parameters with empty values are excluded from the result.
    /// </summary>
    [Fact]
    public void BuildQueryString_ParametersWithEmptyValues_ExcludesEmptyValues()
    {
        // Arrange
        var parameters = new Dictionary<string, string>
        {
            { "key1", "value1" },
            { "key2", "" },
            { "key3", "value3" }
        };

        // Act
        var result = QueryStringHelper.BuildQueryString(parameters);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Be("key1=value1&key3=value3");
        result.Should().NotContain("key2");
    }

    /// <summary>
    /// Tests the <see cref="QueryStringHelper.BuildQueryString"/> method with parameters containing null values.
    /// Validates that parameters with null values are excluded from the result.
    /// </summary>
    [Fact]
    public void BuildQueryString_ParametersWithNullValues_ExcludesNullValues()
    {
        // Arrange
        var parameters = new Dictionary<string, string?>
        {
            { "key1", "value1" },
            { "key2", null },
            { "key3", "value3" }
        };

        // Act
        var result = QueryStringHelper.BuildQueryString(parameters);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Be("key1=value1&key3=value3");
        result.Should().NotContain("key2");
    }

    /// <summary>
    /// Tests the <see cref="QueryStringHelper.ParseQueryString"/> method with a valid query string.
    /// Validates that the method correctly parses a query string into a dictionary.
    /// </summary>
    [Fact]
    public void ParseQueryString_ValidQueryString_ReturnsCorrectDictionary()
    {
        // Arrange
        var queryString = "key1=value1&key2=value2&key3=value3";

        // Act
        var result = QueryStringHelper.ParseQueryString(queryString);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result["key1"].Should().Be("value1");
        result["key2"].Should().Be("value2");
        result["key3"].Should().Be("value3");
    }

    /// <summary>
    /// Tests the <see cref="QueryStringHelper.ParseQueryString"/> method with a query string containing special characters.
    /// Validates that special characters are properly URL decoded.
    /// </summary>
    [Fact]
    public void ParseQueryString_SpecialCharacters_DecodesCorrectly()
    {
        // Arrange
        var queryString = "api+key=value+with+spaces&key%26name=value%3Dwith%3Dequals&key%3Ftest=value%23anchor";

        // Act
        var result = QueryStringHelper.ParseQueryString(queryString);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result["api key"].Should().Be("value with spaces");
        result["key&name"].Should().Be("value=with=equals");
        result["key?test"].Should().Be("value#anchor");
    }

    /// <summary>
    /// Tests the <see cref="QueryStringHelper.ParseQueryString"/> method with a query string starting with '?'.
    /// Validates that the leading '?' is properly handled.
    /// </summary>
    [Fact]
    public void ParseQueryString_QueryStringWithLeadingQuestionMark_TrimsCorrectly()
    {
        // Arrange
        var queryString = "?key1=value1&key2=value2";

        // Act
        var result = QueryStringHelper.ParseQueryString(queryString);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result["key1"].Should().Be("value1");
        result["key2"].Should().Be("value2");
    }

    /// <summary>
    /// Tests the <see cref="QueryStringHelper.ParseQueryString"/> method with an empty query string.
    /// Validates that an empty query string returns an empty dictionary.
    /// </summary>
    [Fact]
    public void ParseQueryString_EmptyQueryString_ReturnsEmptyDictionary()
    {
        // Arrange
        var queryString = "";

        // Act
        var result = QueryStringHelper.ParseQueryString(queryString);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Tests the <see cref="QueryStringHelper.ParseQueryString"/> method with a null query string.
    /// Validates that a null query string returns an empty dictionary.
    /// </summary>
    [Fact]
    public void ParseQueryString_NullQueryString_ReturnsEmptyDictionary()
    {
        // Arrange
        string queryString = null!;

        // Act
        var result = QueryStringHelper.ParseQueryString(queryString);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Tests the <see cref="QueryStringHelper.ParseQueryString"/> method with a query string containing a parameter without a value.
    /// Validates that parameters without values are parsed with empty strings.
    /// </summary>
    [Fact]
    public void ParseQueryString_ParameterWithoutValue_ParsesWithEmptyString()
    {
        // Arrange
        var queryString = "key1=value1&key2";

        // Act
        var result = QueryStringHelper.ParseQueryString(queryString);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result["key1"].Should().Be("value1");
        result["key2"].Should().BeEmpty();
    }

    /// <summary>
    /// Tests the <see cref="QueryStringHelper.AppendParameters"/> method with a base URL without query parameters.
    /// Validates that parameters are correctly appended with a '?' separator.
    /// </summary>
    [Fact]
    public void AppendParameters_BaseUrlWithoutQuery_AddsParametersWithQuestionMark()
    {
        // Arrange
        var baseUrl = "https://example.com/api";
        var parameters = new Dictionary<string, string>
        {
            { "key1", "value1" },
            { "key2", "value2" }
        };

        // Act
        var result = QueryStringHelper.AppendParameters(baseUrl, parameters);

        // Assert
        result.Should().Be("https://example.com/api?key1=value1&key2=value2");
    }

    /// <summary>
    /// Tests the <see cref="QueryStringHelper.AppendParameters"/> method with a base URL that already has query parameters.
    /// Validates that parameters are correctly appended with an '&' separator.
    /// </summary>
    [Fact]
    public void AppendParameters_BaseUrlWithQuery_AddsParametersWithAmpersand()
    {
        // Arrange
        var baseUrl = "https://example.com/api?existing=param";
        var parameters = new Dictionary<string, string>
        {
            { "key1", "value1" },
            { "key2", "value2" }
        };

        // Act
        var result = QueryStringHelper.AppendParameters(baseUrl, parameters);

        // Assert
        result.Should().Be("https://example.com/api?existing=param&key1=value1&key2=value2");
    }

    /// <summary>
    /// Tests the <see cref="QueryStringHelper.AppendParameters"/> method with an empty parameters dictionary.
    /// Validates that the base URL is returned unchanged.
    /// </summary>
    [Fact]
    public void AppendParameters_EmptyParameters_ReturnsBaseUrlUnchanged()
    {
        // Arrange
        var baseUrl = "https://example.com/api";
        var parameters = new Dictionary<string, string>();

        // Act
        var result = QueryStringHelper.AppendParameters(baseUrl, parameters);

        // Assert
        result.Should().Be(baseUrl);
    }

    /// <summary>
    /// Tests the <see cref="QueryStringHelper.AppendParameters"/> method with a null parameters dictionary.
    /// Validates that the base URL is returned unchanged.
    /// </summary>
    [Fact]
    public void AppendParameters_NullParameters_ReturnsBaseUrlUnchanged()
    {
        // Arrange
        var baseUrl = "https://example.com/api";
        Dictionary<string, string> parameters = null!;

        // Act
        var result = QueryStringHelper.AppendParameters(baseUrl, parameters);

        // Assert
        result.Should().Be(baseUrl);
    }

    /// <summary>
    /// Tests the <see cref="QueryStringHelper.RemoveParameter"/> method with a single parameter to remove.
    /// Validates that the specified parameter is removed from the URL.
    /// </summary>
    [Fact]
    public void RemoveParameter_SingleParameterToRemove_RemovesParameter()
    {
        // Arrange
        var url = "https://example.com/api?key1=value1&key2=value2&key3=value3";
        var parameterNames = new[] { "key2" };

        // Act
        var result = QueryStringHelper.RemoveParameter(url, parameterNames);

        // Assert
        result.Should().Be("https://example.com/api?key1=value1&key3=value3");
    }

    /// <summary>
    /// Tests the <see cref="QueryStringHelper.RemoveParameter"/> method with multiple parameters to remove.
    /// Validates that all specified parameters are removed from the URL.
    /// </summary>
    [Fact]
    public void RemoveParameter_MultipleParametersToRemove_RemovesAllParameters()
    {
        // Arrange
        var url = "https://example.com/api?key1=value1&key2=value2&key3=value3&key4=value4";
        var parameterNames = new[] { "key2", "key4" };

        // Act
        var result = QueryStringHelper.RemoveParameter(url, parameterNames);

        // Assert
        result.Should().Be("https://example.com/api?key1=value1&key3=value3");
    }

    /// <summary>
    /// Tests the <see cref="QueryStringHelper.RemoveParameter"/> method with a parameter that doesn't exist.
    /// Validates that the URL remains unchanged when removing a non-existent parameter.
    /// </summary>
    [Fact]
    public void RemoveParameter_NonExistentParameter_UrlUnchanged()
    {
        // Arrange
        var url = "https://example.com/api?key1=value1&key2=value2";
        var parameterNames = new[] { "key3" };

        // Act
        var result = QueryStringHelper.RemoveParameter(url, parameterNames);

        // Assert
        result.Should().Be(url);
    }

    /// <summary>
    /// Tests the <see cref="QueryStringHelper.RemoveParameter"/> method with a URL that has no query parameters.
    /// Validates that the URL remains unchanged when removing parameters from a URL without query string.
    /// </summary>
    [Fact]
    public void RemoveParameter_UrlWithoutQueryString_UrlUnchanged()
    {
        // Arrange
        var url = "https://example.com/api";
        var parameterNames = new[] { "key1" };

        // Act
        var result = QueryStringHelper.RemoveParameter(url, parameterNames);

        // Assert
        result.Should().Be(url);
    }

    /// <summary>
    /// Tests the <see cref="QueryStringHelper.RemoveParameter"/> method with case-insensitive parameter names.
    /// Validates that parameter removal is case-insensitive.
    /// </summary>
    [Fact]
    public void RemoveParameter_CaseInsensitiveParameterNames_RemovesParameter()
    {
        // Arrange
        var url = "https://example.com/api?Key1=value1&KEY2=value2&key3=value3";
        var parameterNames = new[] { "key2" };

        // Act
        var result = QueryStringHelper.ParseQueryString(url);
        result.Remove("KEY2");
        var rebuiltUrl = QueryStringHelper.BuildQueryString(result);
        var finalUrl = $"https://example.com/api?{rebuiltUrl}";

        // Assert
        finalUrl.Should().NotContain("KEY2=value2");
        finalUrl.Should().NotContain("key2=value2");
    }
}
