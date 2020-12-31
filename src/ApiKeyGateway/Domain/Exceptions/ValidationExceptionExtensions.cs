namespace ApiKeyGateway.Domain.Exceptions;

/// <summary>
/// Provides extension methods for <see cref="ValidationException"/>.
/// </summary>
public static class ValidationExceptionExtensions
{
    /// <summary>
    /// Formats a <see cref="ValidationException"/> into a human-readable string.
    /// </summary>
    /// <param name="exception">The <see cref="ValidationException"/> to format.</param>
    /// <returns>A formatted string representation of the exception.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is null.</exception>
    public static string ToFormattedString(this ValidationException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var message = exception.Message;
        if (exception.ParameterName is { } parameterName)
        {
            message += $" (Parameter: {parameterName})";
        }

        if (exception.AttemptedValue is { } attemptedValue)
        {
            message += $" Attempted value: {attemptedValue}";
        }

        if (exception.ValidationErrors is { } validationErrors)
        {
            message += Environment.NewLine + string.Join(Environment.NewLine, validationErrors);
        }

        return message;
    }

    /// <summary>
    /// Determines if a <see cref="ValidationException"/> has any validation errors.
    /// </summary>
    /// <param name="exception">The <see cref="ValidationException"/> to check.</param>
    /// <returns><c>true</c> if the exception has any validation errors; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is null.</exception>
    public static bool HasValidationErrors(this ValidationException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception.ValidationErrors?.Any() ?? false;
    }
}
