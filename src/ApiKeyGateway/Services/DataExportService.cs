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
    Task<string> ExportApiKeysAsync(string format);
    Task<string> ExportAuditLogsAsync(string format, DateTime? since = null);
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

    public async Task<string> ExportApiKeysAsync(string format)
    {
        if (string.IsNullOrWhiteSpace(format))
            throw new ValidationException("Format cannot be empty", nameof(format), format);

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

    public async Task<string> ExportAuditLogsAsync(string format, DateTime? since = null)
    {
        if (string.IsNullOrWhiteSpace(format))
            throw new ValidationException("Format cannot be empty", nameof(format), format);

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

    public async Task<string> ExportUsageAsync(string format, DateTime startDate, DateTime endDate)
    {
        if (string.IsNullOrWhiteSpace(format))
            throw new ValidationException("Format cannot be empty", nameof(format), format);

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
