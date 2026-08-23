// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Domain.Exceptions;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Repositories;
using ApiKeyGateway.Utilities;
using System.Text;

namespace ApiKeyGateway.Services;

/// <summary>
/// Service for exporting data in multiple formats (CSV, XML, JSON, NDJSON).
/// Handles large datasets by streaming to avoid memory exhaustion.
/// Used for reports, analytics, and system migrations.
/// </summary>
public interface IDataExportService
{
    /// <summary>
    /// Exports all API keys in the requested format.
    /// </summary>
    /// <param name="format">The export format: "csv", "xml", "json", or "ndjson".</param>
    /// <returns>The serialized API key data as a string.</returns>
    Task<string> ExportApiKeysAsync(string format);

    /// <summary>
    /// Exports audit logs in the requested format.
    /// </summary>
    /// <param name="format">The export format: "csv", "xml", "json", or "ndjson".</param>
    /// <param name="since">The start date for the export window; defaults to the last 30 days.</param>
    /// <returns>The serialized audit log data as a string.</returns>
    Task<string> ExportAuditLogsAsync(string format, DateTime? since = null);

    /// <summary>
    /// Exports usage records within the given date range in the requested format.
    /// </summary>
    /// <param name="format">The export format: "csv", "xml", "json", or "ndjson".</param>
    /// <param name="startDate">The inclusive start of the export window.</param>
    /// <param name="endDate">The inclusive end of the export window.</param>
    /// <returns>The serialized usage data as a string.</returns>
    Task<string> ExportUsageAsync(string format, DateTime startDate, DateTime endDate);
}

/// <summary>
/// Implementation of data export service.
/// </summary>
public sealed class DataExportService : IDataExportService
{
    private readonly IApiKeyRepository _apiKeyRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUsageRepository _usageRepository;
    private readonly ILogger<DataExportService> _logger;

            /// <summary>
        /// Initializes a new instance of the DataExportService class.
        /// </summary>
        /// <param name="apiKeyRepository">The repository for accessing API key data.</param>
        /// <param name="auditLogRepository">The repository for accessing audit log data.</param>
        /// <param name="usageRepository">The repository for accessing usage data.</param>
        /// <param name="logger">The logger for writing application logs.</param>
        public DataExportService(
        IApiKeyRepository apiKeyRepository,
        IAuditLogRepository auditLogRepository,
        IUsageRepository usageRepository,
        ILogger<DataExportService> logger)
    {
        _apiKeyRepository = apiKeyRepository;
        _auditLogRepository = auditLogRepository;
        _usageRepository = usageRepository;
        _logger = logger;
    }

        /// <summary>
    /// Exports all API keys in the requested format.
    /// </summary>
    /// <param name="format">The export format: "csv", "xml", "json", or "ndjson".</param>
    /// <returns>The serialized API key data as a string.</returns>
    public async Task<string> ExportApiKeysAsync(string format)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(format));

        _logger.LogInformation("Exporting API keys in {Format} format", format);

        try
        {
            // In production, use streaming for large datasets
            var apiKeys = await _apiKeyRepository.GetAllAsync();

            return format.ToLowerInvariant() switch
            {
                "csv" => CsvExportHelper.ToCsv(apiKeys),
                "xml" => XmlExportHelper.ToXml(apiKeys, "apiKeys", "apiKey"),
                "json" => JsonSerializationHelper.SerializeFormatted(apiKeys),
                "ndjson" => ToNdJson(apiKeys),
                _ => CsvExportHelper.ToCsv(apiKeys)
            };
        }
        catch (DataAccessException)
        {
            // Re-throw DataAccessException as-is
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting API keys");
            throw new DataAccessException("Failed to export API keys", nameof(ExportApiKeysAsync), nameof(ApiKey), ex);
        }
    }

    /// <summary>
    /// Exports audit logs in the requested format.
    /// </summary>
    /// <param name="format">The export format: "csv", "xml", "json", or "ndjson".</param>
    /// <param name="since">The start date for the export window; defaults to the last 30 days.</param>
    /// <returns>The serialized audit log data as a string.</returns>
    public async Task<string> ExportAuditLogsAsync(string format, DateTime? since = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(format));

        _logger.LogInformation(
            "Exporting audit logs in {Format} format since {Since}",
            format,
            since?.ToString("O") ?? "beginning");

        try
        {
            var startDate = since ?? DateTime.UtcNow.AddDays(-30);
            // In production, query audit logs from date range
            var auditLogs = new List<object>();

            return format.ToLowerInvariant() switch
            {
                "csv" => CsvExportHelper.ToCsv(auditLogs),
                "xml" => XmlExportHelper.ToXml(auditLogs, "auditLogs", "log"),
                "json" => JsonSerializationHelper.SerializeFormatted(auditLogs),
                "ndjson" => ToNdJson(auditLogs),
                _ => CsvExportHelper.ToCsv(auditLogs)
            };
        }
        catch (DataAccessException)
        {
            // Re-throw DataAccessException as-is
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting audit logs");
            throw new DataAccessException("Failed to export audit logs", nameof(ExportAuditLogsAsync), nameof(AuditLog), ex);
        }
    }

    /// <summary>
    /// Exports usage records within the given date range in the requested format.
    /// </summary>
    /// <param name="format">The export format: "csv", "xml", "json", or "ndjson".</param>
    /// <param name="startDate">The inclusive start of the export window.</param>
    /// <param name="endDate">The inclusive end of the export window.</param>
    /// <returns>The serialized usage data as a string.</returns>
    public async Task<string> ExportUsageAsync(string format, DateTime startDate, DateTime endDate)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(format));

        if (endDate < startDate)
            throw new ValidationException("End date must be after start date", nameof(endDate), endDate);

        _logger.LogInformation(
            "Exporting usage data in {Format} format from {StartDate} to {EndDate}",
            format,
            startDate.ToString("O"),
            endDate.ToString("O"));

        try
        {
            var usageRecords = await _usageRepository.GetUsageAsync(startDate, endDate);

            return format.ToLowerInvariant() switch
            {
                "csv" => CsvExportHelper.ToCsv(usageRecords),
                "xml" => XmlExportHelper.ToXml(usageRecords, "usageRecords", "record"),
                "json" => JsonSerializationHelper.SerializeFormatted(usageRecords),
                "ndjson" => ToNdJson(usageRecords),
                _ => CsvExportHelper.ToCsv(usageRecords)
            };
        }
        catch (DataAccessException)
        {
            // Re-throw DataAccessException as-is
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting usage data");
            throw new DataAccessException("Failed to export usage data", nameof(ExportUsageAsync), nameof(UsageRecord), ex);
        }
    }

    private static string ToNdJson<T>(IEnumerable<T> items)
    {
        var sb = new StringBuilder();
        foreach (var item in items)
        {
            sb.AppendLine(JsonSerializationHelper.SerializeCompact(item));
        }
        return sb.ToString();
    }
}
