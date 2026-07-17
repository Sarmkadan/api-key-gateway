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

        if (exception.ParameterName is { Length: > 0 } parameterName)
        {
            message += $" (Parameter: {parameterName})";
        }

        if (exception.AttemptedValue is not null)
        {
            message += $" Attempted value: {exception.AttemptedValue}";
        }

        if (exception.ValidationErrors?.Any() == true)
        {
            message += Environment.NewLine + string.Join(Environment.NewLine, exception.ValidationErrors);
        }

        return message;
    }

    /// <summary>
    /// Determines if a <see cref="ValidationException"/> contains any validation errors.
    /// </summary>
    /// <param name="exception">The <see cref="ValidationException"/> to check for validation errors.</param>
    /// <returns><c>true</c> if the exception contains one or more validation errors; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is <see langword="null"/>.</exception>
    public static bool HasValidationErrors(this ValidationException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception.ValidationErrors?.Any() == true;
    }
}