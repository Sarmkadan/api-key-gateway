// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Utilities;

/// <summary>
/// Helper for consistent logging setup across the application.
/// Configures structured logging with appropriate levels and outputs.
/// Ensures sensitive information is redacted from logs.
/// </summary>
public static class LoggerFactoryHelper
{
    /// <summary>
    /// Configures logging for the application with structured logging.
    /// </summary>
    public static ILoggingBuilder ConfigureGatewayLogging(
        this ILoggingBuilder logging,
        IConfiguration configuration,
        string environmentName)
    {
        var logLevel = configuration["Logging:LogLevel:Default"] ?? "Information";

        // Clear default providers and add console with color
        logging.ClearProviders();
        logging.AddConsole();
        logging.AddDebug();

        // Set minimum log level
        if (Enum.TryParse<LogLevel>(logLevel, out var level))
        {
            logging.SetMinimumLevel(level);
        }

        // Add more verbose logging in development
        if (environmentName == "Development")
        {
            logging.AddDebug();
            logging.SetMinimumLevel(LogLevel.Debug);
        }

        return logging;
    }

    /// <summary>
    /// Masks sensitive data in log messages (e.g., API keys).
    /// </summary>
    public static string MaskSensitiveData(string value)
    {
        if (string.IsNullOrEmpty(value) || value.Length < 8)
            return "***";

        // Show first and last few characters, mask middle
        var prefix = value.Substring(0, Math.Min(4, value.Length));
        var suffix = value.Length > 8 ? value.Substring(value.Length - 4) : string.Empty;

        return $"{prefix}...{suffix}";
    }

    /// <summary>
    /// Creates a structured log message with context.
    /// </summary>
    public static string FormatLogMessage(
        string action,
        string resource,
        Dictionary<string, object>? context = null)
    {
        var message = $"[{action}] {resource}";

        if (context?.Count > 0)
        {
            var details = string.Join(", ", context.Select(x => $"{x.Key}={x.Value}"));
            message += $" | {details}";
        }

        return message;
    }
}
