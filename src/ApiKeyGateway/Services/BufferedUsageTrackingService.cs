// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Domain.Exceptions;
using ApiKeyGateway.Domain.Models;
using System.Threading.Channels;

namespace ApiKeyGateway.Services;

/// <summary>
/// Buffered implementation of IUsageTrackingService that accumulates usage records
/// and writes them in batches to reduce database round-trips.
/// </summary>
/// <remarks>
/// This decorator wraps an IUsageTrackingService and buffers RecordUsageAsync calls
/// in memory, flushing them to the underlying repository in configurable batch sizes
/// and intervals to improve performance in high-throughput scenarios.
/// </remarks>
public sealed class BufferedUsageTrackingService : IUsageTrackingService, IAsyncDisposable
{
    private readonly IUsageTrackingService _innerService;
    private readonly IUsageRepository _repository;
    private readonly BufferedUsageTrackingOptions _options;
    private readonly ILogger<BufferedUsageTrackingService> _logger;
    private readonly CancellationTokenSource _shutdownTokenSource = new();
    private readonly System.Threading.Channels.Channel<UsageRecord> _channel;
    private Task? _flushTask;

    /// <summary>
    /// Configuration options for buffered usage tracking
    /// </summary>
    public sealed class BufferedUsageTrackingOptions
    {
        /// <summary>
        /// Maximum number of records to buffer before flushing to repository
        /// </summary>
        public int MaxBatchSize { get; set; } = 100;

        /// <summary>
        /// Maximum time to wait before flushing buffered records
        /// </summary>
        public TimeSpan MaxFlushInterval { get; set; } = TimeSpan.FromSeconds(2);

        /// <summary>
        /// Channel capacity - controls behavior when buffer is full:
        /// - BoundedChannelFullMode.Wait: blocks when full (default)
        /// - BoundedChannelFullMode.DropNewest: drops newest when full
        /// - BoundedChannelFullMode.DropOldest: drops oldest when full
        /// </summary>
        public BoundedChannelFullMode ChannelFullMode { get; set; } = BoundedChannelFullMode.Wait;

        /// <summary>
        /// Channel capacity - maximum number of records to hold in memory
        /// </summary>
        public int ChannelCapacity { get; set; } = 10000;
    }

    /// <summary>
    /// Initializes a new instance of BufferedUsageTrackingService
    /// </summary>
    /// <param name="innerService">The inner usage tracking service to delegate to</param>
    /// <param name="repository">The repository for batch writes</param>
    /// <param name="options">Configuration options for buffering</param>
    /// <param name="logger">Logger instance</param>
    public BufferedUsageTrackingService(
        IUsageTrackingService innerService,
        IUsageRepository repository,
        BufferedUsageTrackingOptions? options = null,
        ILogger<BufferedUsageTrackingService>? logger = null)
    {
        _innerService = innerService ?? throw new ArgumentNullException(nameof(innerService));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _options = options ?? new BufferedUsageTrackingOptions();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Create bounded channel with specified capacity and overflow behavior
        _channel = System.Threading.Channels.Channel.CreateBounded<UsageRecord>(
            new BoundedChannelOptions(_options.ChannelCapacity)
            {
                FullMode = _options.ChannelFullMode,
                SingleReader = true,
                SingleWriter = false
            });

        // Start background flusher task
        _flushTask = StartBackgroundFlusherAsync(_shutdownTokenSource.Token);
    }

    /// <summary>
    /// Records a usage entry by adding it to the buffer
    /// </summary>
    /// <param name="record">The usage record to record</param>
    /// <exception cref="ArgumentNullException">Thrown if record is null</exception>
    public async Task RecordUsageAsync(UsageRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        try
        {
            await _channel.Writer.WriteAsync(record, _shutdownTokenSource.Token);
            _logger.LogTrace("Usage record buffered for API key {ApiKeyId}", record.ApiKeyId);
        }
        catch (OperationCanceledException) when (_shutdownTokenSource.IsCancellationRequested)
        {
            // During shutdown, fall back to immediate write
            _logger.LogDebug("Shutdown in progress, falling back to immediate write for API key {ApiKeyId}", record.ApiKeyId);
            await _innerService.RecordUsageAsync(record);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to buffer usage record for API key {ApiKeyId}", record.ApiKeyId);
            throw;
        }
    }

    /// <summary>
    /// Retrieves usage statistics for an API key within a date range
    /// </summary>
    /// <param name="apiKeyId">The API key identifier</param>
    /// <param name="startDate">Start date of the range</param>
    /// <param name="endDate">End date of the range</param>
    /// <returns>Usage statistics</returns>
    /// <exception cref="ValidationException">Thrown if parameters are invalid</exception>
    public Task<UsageStatistics> GetUsageStatisticsAsync(string apiKeyId, DateTime startDate, DateTime endDate)
        => _innerService.GetUsageStatisticsAsync(apiKeyId, startDate, endDate);

    /// <summary>
    /// Retrieves detailed usage records for an API key
    /// </summary>
    /// <param name="apiKeyId">The API key identifier</param>
    /// <param name="startDate">Start date of the range</param>
    /// <param name="endDate">End date of the range</param>
    /// <returns>List of usage records</returns>
    /// <exception cref="ValidationException">Thrown if parameters are invalid</exception>
    public Task<List<UsageRecord>> GetUsageRecordsAsync(string apiKeyId, DateTime startDate, DateTime endDate)
        => _innerService.GetUsageRecordsAsync(apiKeyId, startDate, endDate);

    /// <summary>
    /// Calculates total bytes transferred by a consumer
    /// </summary>
    /// <param name="consumerId">The consumer identifier</param>
    /// <param name="startDate">Start date of the range</param>
    /// <param name="endDate">End date of the range</param>
    /// <returns>Total bytes transferred</returns>
    public Task<long> GetTotalBytesUsedAsync(string consumerId, DateTime startDate, DateTime endDate)
        => _innerService.GetTotalBytesUsedAsync(consumerId, startDate, endDate);

    /// <summary>
    /// Retrieves all usage records across all API keys within a date range
    /// </summary>
    /// <param name="startDate">Start date of the range</param>
    /// <param name="endDate">End date of the range</param>
    /// <returns>List of usage records</returns>
    /// <exception cref="ValidationException">Thrown if parameters are invalid</exception>
    public Task<List<UsageRecord>> GetUsageAsync(DateTime startDate, DateTime endDate)
        => _innerService.GetUsageAsync(startDate, endDate);

    /// <summary>
    /// Starts the background flusher task that processes buffered records
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for graceful shutdown</param>
    /// <returns>Task representing the background flusher</returns>
    private async Task StartBackgroundFlusherAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting buffered usage tracking flusher with batch size {MaxBatchSize} and interval {MaxFlushInterval}",
            _options.MaxBatchSize, _options.MaxFlushInterval);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Wait for batch size to be reached
                var batch = await ReadBatchAsync(cancellationToken);

                if (batch.Count > 0)
                {
                    await FlushBatchAsync(batch);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Background flusher shutting down gracefully");
            // Graceful shutdown - flush any remaining records
            var remainingBatch = await ReadBatchAsync(cancellationToken);
            if (remainingBatch.Count > 0)
            {
                await FlushBatchAsync(remainingBatch);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Background flusher encountered an error");
            throw;
        }
        finally
        {
            _logger.LogInformation("Background flusher stopped");
        }
    }

    /// <summary>
    /// Reads a batch of records from the channel
    /// </summary>
    private async Task<List<UsageRecord>> ReadBatchAsync(CancellationToken cancellationToken)
    {
        var batch = new List<UsageRecord>(_options.MaxBatchSize);
        var readCount = 0;

        while (readCount < _options.MaxBatchSize && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Try to read immediately first
                if (_channel.Reader.TryRead(out var record))
                {
                    batch.Add(record);
                    readCount++;
                }
                else
                {
                    // Wait for more data with timeout to allow periodic flushing
                    var timeoutTask = Task.Delay(_options.MaxFlushInterval, cancellationToken);
                    var waitTask = _channel.Reader.WaitToReadAsync(cancellationToken).AsTask();

                    var completedTask = await Task.WhenAny(timeoutTask, waitTask);

                    if (completedTask == waitTask && await waitTask)
                    {
                        // Channel has data available, try reading again
                        if (_channel.Reader.TryRead(out record))
                        {
                            batch.Add(record);
                            readCount++;
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        return batch;
    }

    /// <summary>
    /// Flushes a batch of records to the repository
    /// </summary>
    private async Task FlushBatchAsync(List<UsageRecord> batch)
    {
        if (batch.Count == 0)
            return;

        try
        {
            _logger.LogDebug("Flushing {Count} buffered usage records to repository", batch.Count);
            await _repository.WriteBatchAsync(batch);
            _logger.LogTrace("Successfully flushed {Count} usage records", batch.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to flush batch of {Count} usage records", batch.Count);

            // Fallback: write each record individually if batch write fails
            foreach (var record in batch)
            {
                try
                {
                    await _innerService.RecordUsageAsync(record);
                }
                catch (Exception innerEx)
                {
                    _logger.LogError(innerEx, "Failed to record individual usage for API key {ApiKeyId}", record.ApiKeyId);
                }
            }
        }
    }

    /// <summary>
    /// Disposes the service and flushes any remaining buffered records
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_flushTask != null)
        {
            try
            {
                _shutdownTokenSource.Cancel();

                // Wait for flusher to complete
                if (_flushTask.IsCompleted == false)
                {
                    await _flushTask;
                }
            }
            catch (OperationCanceledException)
            {
                // Expected during shutdown
            }
            finally
            {
                _shutdownTokenSource.Dispose();
                _channel.Writer.Complete();
            }
        }
    }
}