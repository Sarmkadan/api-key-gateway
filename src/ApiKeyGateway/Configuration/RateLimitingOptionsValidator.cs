// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Options;

namespace ApiKeyGateway.Configuration;

/// <summary>
/// Startup validator for <see cref="RateLimitingOptions"/>. Registered with
/// <c>ValidateOnStart</c> so an unknown policy reference or invalid limit fails the host
/// before it accepts traffic, with every error listed in the exception message.
/// </summary>
public sealed class RateLimitingOptionsValidator : IValidateOptions<RateLimitingOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, RateLimitingOptions options)
    {
        var errors = RateLimitingOptionsValidation.Validate(options);

        return errors.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(errors);
    }
}
