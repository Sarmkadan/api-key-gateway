using System;
using System.Collections.Generic;
using ApiKeyGateway.Utilities;
using Xunit;

namespace api_key_gateway.Tests;

public class JsonSerializationHelperValidationTests
{
    [Fact]
    public void Validate_ReturnsEmptyList_WhenHelperBehavesCorrectly()
    {
        // Act
        IReadOnlyList<string> result = JsonSerializationHelperValidation.Validate();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void IsValid_ReturnsTrue_WhenValidateReturnsNoErrors()
    {
        // Act
        bool isValid = JsonSerializationHelperValidation.IsValid();

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void EnsureValid_DoesNotThrow_WhenHelperIsValid()
    {
        // Act & Assert
        var exception = Record.Exception(() => JsonSerializationHelperValidation.EnsureValid());
        Assert.Null(exception);
    }

    [Fact]
    public void Validate_ReturnsReadOnlyList_AndCannotBeModified()
    {
        // Act
        IReadOnlyList<string> result = JsonSerializationHelperValidation.Validate();

        // Assert that the returned list implements IReadOnlyList
        Assert.IsAssignableFrom<IReadOnlyList<string>>(result);

        // Attempt to cast to IList and modify – should throw NotSupportedException
        var asList = Assert.IsAssignableFrom<IList<string>>(result);
        Assert.Throws<NotSupportedException>(() => asList.Add("unexpected"));
    }
}
