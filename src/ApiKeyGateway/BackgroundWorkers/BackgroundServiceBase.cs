using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ApiKeyGateway.BackgroundWorkers;

/// <summary>
/// Abstract base class for background workers that provides common timer loop,
/// error handling, and cancellation token checking functionality.
/// </summary>
/// <typeparam name="TWorker">The concrete worker type for logging purposes.</typeparam>
public abstract class BackgroundServiceBase<TWorker> : BackgroundService where TWorker : BackgroundServiceBase<TWorker>
{
    protected readonly IServiceProvider _serviceProvider;
    protected readonly ILogger<TWorker> _logger;
    protected readonly TimeSpan _defaultDelay = TimeSpan.FromMinutes(1);

    protected BackgroundServiceBase(
        IServiceProvider serviceProvider,
        ILogger<TWorker> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Main execution loop that handles timer logic, cancellation, and error handling.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("{WorkerName} started", typeof(TWorker).Name);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ExecuteCycleAsync(stoppingToken);
                await Task.Delay(_defaultDelay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("{WorkerName} shutting down", typeof(TWorker).Name);
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in {WorkerName}", typeof(TWorker).Name);
                await Task.Delay(GetErrorDelay(stoppingToken), stoppingToken);
            }
        }
    }

    /// <summary>
    /// Executes a single cycle of the worker's main logic.
    /// </summary>
    protected abstract Task ExecuteCycleAsync(CancellationToken stoppingToken);

    /// <summary>
    /// Gets the delay to use after an error occurs.
    /// Override this method to provide custom error delay behavior.
    /// </summary>
    protected virtual TimeSpan GetErrorDelay(CancellationToken stoppingToken)
    {
        return TimeSpan.FromMinutes(5);
    }

    /// <summary>
    /// Creates a new service scope for resolving scoped dependencies.
    /// </summary>
    protected IServiceScope CreateScope()
    {
        return _serviceProvider.CreateScope();
    }
}