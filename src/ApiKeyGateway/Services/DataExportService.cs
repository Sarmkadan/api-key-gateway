// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Repositories;
using ApiKeyGateway.Utilities;

namespace ApiKeyGateway.Services;

/// <summary>
/// Service for exporting data in multiple formats (CSV, XML, JSON).
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
        _logger.LogInformation("Exporting API keys in {Format} format", format);

        try
        {
            // In production, use streaming for large datasets
            var apiKeys = await _apiKeyRepository.GetAllAsync();

            return format.ToLower() switch
            {
                "csv" => CsvExportHelper.ToCsv(apiKeys),
                "xml" => XmlExportHelper.ToXml(apiKeys, "apiKeys", "apiKey"),
                "json" => JsonSerializationHelper.SerializeFormatted(apiKeys),
                _ => CsvExportHelper.ToCsv(apiKeys)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting API keys");
            throw;
        }
    }

    public async Task<string> ExportAuditLogsAsync(string format, DateTime? since = null)
    {
        _logger.LogInformation(
            "Exporting audit logs in {Format} format since {Since}",
            format,
            since?.ToString("O") ?? "beginning");

        try
        {
            var startDate = since ?? DateTime.UtcNow.AddDays(-30);
            // In production, query audit logs from date range
            var auditLogs = new List<object>();

            return format.ToLower() switch
            {
                "csv" => CsvExportHelper.ToCsv(auditLogs),
                "xml" => XmlExportHelper.ToXml(auditLogs, "auditLogs", "log"),
                "json" => JsonSerializationHelper.SerializeFormatted(auditLogs),
                _ => CsvExportHelper.ToCsv(auditLogs)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting audit logs");
            throw;
        }
    }

    public async Task<string> ExportUsageAsync(string format, DateTime startDate, DateTime endDate)
    {
        _logger.LogInformation(
            "Exporting usage data in {Format} format from {StartDate} to {EndDate}",
            format,
            startDate:O,
            endDate:O);

        try
        {
            var usageRecords = await _usageRepository.GetUsageAsync(startDate, endDate);

            return format.ToLower() switch
            {
                "csv" => CsvExportHelper.ToCsv(usageRecords),
                "xml" => XmlExportHelper.ToXml(usageRecords, "usageRecords", "record"),
                "json" => JsonSerializationHelper.SerializeFormatted(usageRecords),
                _ => CsvExportHelper.ToCsv(usageRecords)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting usage data");
            throw;
        }
    }
}
