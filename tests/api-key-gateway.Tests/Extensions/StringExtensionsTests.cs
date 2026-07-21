using Xunit;
using ApiKeyGateway.Extensions;

namespace api_key_gateway.Tests.Extensions;

public class StringExtensionsTests
{
    [Theory]
    [InlineData(null, 10, null)]
    [InlineData("", 10, "")]
    [InlineData("short", 10, "short")]
    [InlineData("longerstring", 5, "longe")]
    public void Truncate_ReturnsExpectedResult(string? input, int maxLength, string? expected)
    {
        // Act
        var result = input.Truncate(maxLength);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Truncate_ThrowsOnNegativeLength()
    {
        // Arrange
        var input = "test";

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => input.Truncate(-1));
    }

    [Theory]
    [InlineData(null, 10, null)]
    [InlineData("", 10, "")]
    [InlineData("short", 10, "short")]
    [InlineData("longerstring", 8, "longe...")]
    public void TruncateWithEllipsis_ReturnsExpectedResult(string? input, int maxLength, string? expected)
    {
        // Act
        var result = input.TruncateWithEllipsis(maxLength);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void TruncateWithEllipsis_ThrowsOnInvalidLength()
    {
        // Arrange
        var input = "test";

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => input.TruncateWithEllipsis(2));
    }

    [Theory]
    [InlineData(null, new[] { "test" }, false)]
    [InlineData("Hello World", new[] { "hello" }, true)]
    [InlineData("Hello World", new[] { "world" }, true)]
    [InlineData("Hello World", new[] { "foo" }, false)]
    public void ContainsAny_ReturnsExpectedResult(string? input, string[] searches, bool expected)
    {
        // Act
        var result = input.ContainsAny(searches);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("Hello World", "hello-world")]
    [InlineData("Hello  World", "hello-world")]
    [InlineData("  Hello-World  ", "hello-world")]
    [InlineData("Hello!!!World", "helloworld")]
    public void ToSlug_ReturnsExpectedResult(string? input, string expected)
    {
        // Act
        var result = input.ToSlug();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("hello", "Hello")]
    [InlineData("h", "H")]
    public void CapitalizeFirst_ReturnsExpectedResult(string? input, string? expected)
    {
        // Act
        var result = input.CapitalizeFirst();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("123", true)]
    [InlineData("12a", false)]
    public void IsNumeric_ReturnsExpectedResult(string? input, bool expected)
    {
        // Act
        var result = input.IsNumeric();

        // Assert
        Assert.Equal(expected, result);
    }
}
