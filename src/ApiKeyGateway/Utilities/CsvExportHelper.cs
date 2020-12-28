// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using System.Reflection;
using System.Text;

namespace ApiKeyGateway.Utilities;

/// <summary>
/// Helper class to convert collections of objects to CSV format.
/// Used for exporting usage records, audit logs, and API key metrics
/// to external analytics tools or for user downloads.
/// </summary>
public static class CsvExportHelper
{
    /// <summary>
    /// Converts a collection of objects to CSV format with proper escaping.
    /// Headers are derived from public properties of the object type.
    /// </summary>
    public static string ToCsv<T>(IEnumerable<T> items, bool includeHeaders = true)
    {
        if (items == null || !items.Any())
            return string.Empty;

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var sb = new StringBuilder();

        // Write headers if requested
        if (includeHeaders)
        {
            var headers = properties
                .Select(p => QuoteCsvField(p.Name))
                .ToList();
            sb.AppendLine(string.Join(",", headers));
        }

        // Write data rows
        foreach (var item in items)
        {
            var values = properties
                .Select(p => QuoteCsvField(p.GetValue(item)))
                .ToList();
            sb.AppendLine(string.Join(",", values));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Exports to CSV and writes directly to a stream.
    /// Useful for large datasets that shouldn't be held in memory.
    /// </summary>
    public static async Task ExportToCsvAsync<T>(
        IAsyncEnumerable<T> items,
        Stream outputStream,
        bool includeHeaders = true)
    {
        using var writer = new StreamWriter(outputStream, Encoding.UTF8, leaveOpen: true);
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        if (includeHeaders)
        {
            var headers = properties
                .Select(p => QuoteCsvField(p.Name))
                .ToList();
            await writer.WriteLineAsync(string.Join(",", headers));
        }

        // Process items as they arrive instead of buffering
        await foreach (var item in items)
        {
            var values = properties
                .Select(p => QuoteCsvField(p.GetValue(item)))
                .ToList();
            await writer.WriteLineAsync(string.Join(",", values));
        }

        await writer.FlushAsync();
    }

    /// <summary>
    /// Properly escapes and quotes CSV fields according to RFC 4180 standard.
    /// Handles commas, quotes, and newlines within field values.
    /// </summary>
    private static string QuoteCsvField(object? value)
    {
        if (value == null)
            return string.Empty;

        var fieldValue = value switch
        {
            DateTime dt => dt.ToString("O", CultureInfo.InvariantCulture),
            DateTimeOffset dto => dto.ToString("O", CultureInfo.InvariantCulture),
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty
        };

        // Quote field if it contains comma, quote, or newline
        if (fieldValue.Contains(',') || fieldValue.Contains('"') || fieldValue.Contains('\n') || fieldValue.Contains('\r'))
        {
            return $"\"{fieldValue.Replace("\"", "\"\"")}\"";
        }

        return fieldValue;
    }
}
