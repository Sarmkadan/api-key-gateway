// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Domain.Models;
using System.Collections.Concurrent;

namespace ApiKeyGateway.Services;

/// <summary>
/// Coalesces duplicate concurrent requests for the same logical resource into a single upstream
/// operation, sharing its result across all waiting callers to reduce load on downstream services
/// such as databases, caches, or external APIs.
/// </summary>
/// <remarks>
/// When two or more concurrent callers invoke <see cref="ExecuteAsync{T}"/> with the same
/// <c>requestKey</c>, only the first ("leader") executes the provided operation. All subsequent
/// callers ("followers") attach to the leader's in-flight task and receive the same result once
/// the leader completes. If the leader faults or is cancelled, all followers observe the same
/// exception or cancellation.
/// </remarks>
public interface IRequestCoalescingService
{
    /// <summary>
    /// Executes <paramref name="operation"/> if no identical request is currently in flight;
    /// otherwise waits for the in-flight request to complete and returns its shared result.
    /// </summary>
    /// <typeparam name="T">The result type produced by the operation.</typeparam>
    /// <param name="requestKey">
    /// A unique string key that identifies the logical request (e.g. a cache key, a hashed API key,
    /// or a resource identifier). Callers with the same key are coalesced.
    /// </param>
    /// <param name="operation">
    /// A factory delegate that performs the actual work. Invoked only by the leading caller;
    /// followers share its result without invoking this delegate.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel a waiting follower. Cancellation of a follower does not abort the
    /// leader's operation; other followers continue to wait.
    /// </param>
    /// <returns>The result produced by the leading operation.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="requestKey"/> is null or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="operation"/> is null.</exception>
    Task<T> ExecuteAsync<T>(
        string requestKey,
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a snapshot of coalescing metrics for observability and diagnostics.
    /// </summary>
    CoalescingMetrics GetMetrics();
}

/// <summary>
/// Thread-safe singleton implementation of <see cref="IRequestCoalescingService"/> backed by a
/// <see cref="ConcurrentDictionary{TKey,TValue}"/> and <see cref="TaskCompletionSource{TResult}"/>.
/// </summary>
public sealed class RequestCoalescingService : IRequestCoalescingService, IDisposable
{
    private readonly ConcurrentDictionary<string, PendingEntry> _pending =
        new(StringComparer.Ordinal);

    private readonly ILogger<RequestCoalescingService> _logger;
    private long _totalRequests;
    private long _coalescedRequests;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of <see cref="RequestCoalescingService"/>.
    /// </summary>
    /// <param name="logger">Logger used for structured diagnostic output.</param>
    public RequestCoalescingService(ILogger<RequestCoalescingService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<T> ExecuteAsync<T>(
        string requestKey,
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(requestKey))
            throw new ArgumentException("Request key cannot be empty", nameof(requestKey));

        ArgumentNullException.ThrowIfNull(operation);
        cancellationToken.ThrowIfCancellationRequested();

        Interlocked.Increment(ref _totalRequests);

        var entry = new PendingEntry();

        // Attempt to become the leader for this key.
        if (_pending.TryAdd(requestKey, entry))
        {
            _logger.LogDebug(
                "Leading coalesced request for key {RequestKey}", requestKey);

            try
            {
                var result = await operation(cancellationToken);
                entry.TrySetResult(result);
                return result;
            }
            catch (OperationCanceledException)
            {
                entry.TrySetCanceled();
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Coalesced operation failed for key {RequestKey}", requestKey);

                entry.TrySetException(ex);
                throw;
            }
            finally
            {
                _pending.TryRemove(requestKey, out _);
            }
        }

        // Another caller is already leading — join its in-flight task.
        if (_pending.TryGetValue(requestKey, out var existing))
        {
            Interlocked.Increment(ref _coalescedRequests);

            _logger.LogDebug(
                "Joining in-flight coalesced request for key {RequestKey}", requestKey);

            var boxed = await existing.Task.WaitAsync(cancellationToken);
            return (T)boxed!;
        }

        // The leader completed between our failed TryAdd and TryGetValue — execute independently.
        _logger.LogDebug(
            "Falling back to independent execution for key {RequestKey}", requestKey);

        return await operation(cancellationToken);
    }

    /// <inheritdoc />
    public CoalescingMetrics GetMetrics() => new()
    {
        TotalRequests = Interlocked.Read(ref _totalRequests),
        CoalescedRequests = Interlocked.Read(ref _coalescedRequests),
        ActiveRequests = _pending.Count
    };

    /// <summary>
    /// Disposes the service and cancels any pending in-flight entries so waiting followers do not
    /// block indefinitely during application shutdown.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        foreach (var entry in _pending.Values)
            entry.TrySetCanceled();

        _pending.Clear();
    }

    /// <summary>
    /// Wraps a <see cref="TaskCompletionSource{T}"/> so followers can attach to the leader's result
    /// without being aware of the underlying generic type.
    /// </summary>
    private sealed class PendingEntry
    {
        private readonly TaskCompletionSource<object?> _tcs =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        /// <summary>Gets the task followers await.</summary>
        public Task<object?> Task => _tcs.Task;

        /// <summary>Completes the entry with a successful result.</summary>
        public bool TrySetResult(object? result) => _tcs.TrySetResult(result);

        /// <summary>Faults all waiting followers with the given exception.</summary>
        public bool TrySetException(Exception ex) => _tcs.TrySetException(ex);

        /// <summary>Cancels all waiting followers.</summary>
        public bool TrySetCanceled() => _tcs.TrySetCanceled();
    }
}
