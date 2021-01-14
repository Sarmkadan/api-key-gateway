namespace ApiKeyGateway.Utilities;

/// <summary>
/// Extension methods for <see cref="RetryPolicyBuilder"/>.
/// </summary>
public static class RetryPolicyBuilderExtensions
{
    /// <summary>
    /// Adds multiple exception types that should trigger a retry.
    /// </summary>
    /// <param name="builder">The retry policy builder.</param>
    /// <param name="exceptionTypes">The exception types to retry on.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="exceptionTypes"/> is null.</exception>
    public static RetryPolicyBuilder RetryOn(this RetryPolicyBuilder builder, params Type[] exceptionTypes)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(exceptionTypes);

        foreach (var exceptionType in exceptionTypes)
        {
            if (!typeof(Exception).IsAssignableFrom(exceptionType))
            {
                throw new ArgumentException($"The type {exceptionType.Name} does not inherit from Exception.", nameof(exceptionTypes));
            }

            builder.RetryOn(exceptionType);
        }

        return builder;
    }

    /// <summary>
    /// Configures the retry policy to retry on all <see cref="Exception"/> instances.
    /// </summary>
    /// <param name="builder">The retry policy builder.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is null.</exception>
    public static RetryPolicyBuilder RetryOnAllExceptions(this RetryPolicyBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.RetryOn(typeof(Exception));
    }
}
