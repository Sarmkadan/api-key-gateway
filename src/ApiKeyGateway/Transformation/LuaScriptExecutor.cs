using ApiKeyGateway.Configuration;
using ApiKeyGateway.Events;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Diagnostics;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Loaders;

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Transformation;

/// <summary>
/// Executes sandboxed Lua scripts within the request transformation pipeline.
/// Each invocation receives an isolated MoonSharp environment populated with the
/// current <see cref="TransformationContext"/>; mutations made inside the script are
/// reflected back into the context after execution completes.
/// </summary>
public interface ILuaScriptExecutor
{
    /// <summary>
    /// Executes the supplied Lua <paramref name="script"/> against the provided
    /// <paramref name="context"/>. The script receives a <c>request</c> global table
    /// with sub-tables for <c>headers</c>, <c>query</c>, and scalar fields
    /// (<c>body</c>, <c>method</c>, <c>path</c>, <c>ip</c>, <c>consumer_id</c>,
    /// <c>api_key_id</c>). All mutations to these tables are written back to
    /// <paramref name="context"/> on return.
    /// </summary>
    /// <param name="script">Raw Lua source code to execute.</param>
    /// <param name="context">
    /// The transformation context exposed to the script. Mutations are reflected here.
    /// </param>
    /// <param name="cancellationToken">
    /// Used to enforce the per-script wall-clock timeout configured in
    /// <see cref="LuaExecutionOptions.MaxExecutionMs"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the script completed normally and the request should proceed;
    /// <see langword="false"/> when the script signalled a block by returning the string
    /// literal <c>"BLOCK"</c>, which also sets <see cref="TransformationContext.IsBlocked"/>.
    /// </returns>
    /// <exception cref="TransformationScriptException">
    /// Thrown when the Lua runtime raises an unhandled error during script execution.
    /// </exception>
    Task<bool> ExecuteAsync(
        string script,
        TransformationContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs static validation of Lua syntax and checks for access to disallowed
    /// globals without executing the script. Safe to call from an admin API endpoint
    /// before persisting a new rule.
    /// </summary>
    /// <param name="script">Raw Lua source code to validate.</param>
    /// <returns>
    /// A <see cref="ScriptValidationResult"/> containing any syntax errors or security
    /// violations detected during static analysis.
    /// </returns>
    ScriptValidationResult Validate(string script);
}

/// <summary>
/// MoonSharp-backed implementation of <see cref="ILuaScriptExecutor"/>.
/// Runs each script in an isolated <see cref="Script"/> instance with a stripped-down
/// set of core modules to prevent file-system access, process execution, and network I/O.
/// </summary>
/// <remarks>
/// <para>
/// The sandbox exposes only <c>Basic</c>, <c>String</c>, <c>Table</c>, <c>Math</c>,
/// and <c>Bit32</c> module sets. OS, IO, debug, and dynamic-load facilities are excluded.
/// </para>
/// <para>
/// Script execution is delegated to a <see cref="Task.Run(System.Action)"/> worker thread so the calling
/// async context is not blocked. A linked <see cref="CancellationTokenSource"/> enforces the
/// wall-clock timeout. The worker is not abandoned: a debugger-based instruction hook polls the
/// token between Lua instructions, so a runaway script (including a tight loop) is unwound on its
/// own thread instead of spinning after the caller has given up.
/// </para>
/// <para>
/// Each invocation allocates a fresh <see cref="Script"/>, so no globals, metatables, or
/// registry entries survive from one request or API key to the next.
/// </para>
/// <para>
/// The managed allocation budget <see cref="LuaExecutionOptions.MaxMemoryBytes"/> is checked by the
/// same hook. Exceeding it terminates the script and quarantines it after
/// <see cref="LuaExecutionOptions.QuarantineThreshold"/> consecutive failures.
/// </para>
/// </remarks>
public sealed class LuaScriptExecutor : ILuaScriptExecutor
{
    private static readonly CoreModules SandboxModules =
        CoreModules.Basic
        | CoreModules.String
        | CoreModules.Table
        | CoreModules.Math
        | CoreModules.Bit32;

    private readonly LuaExecutionOptions _options;
    private readonly ILogger<LuaScriptExecutor> _logger;
    private readonly IEventPublisher _eventPublisher;
    private readonly Dictionary<string, (int failureCount, DateTime quarantineUntil)> _quarantineRegistry = new();

    // Built-in globals that are always stripped from the sandbox environment.
    private static readonly string[] _builtinBlockedGlobals =
    ["dofile", "loadfile", "load", "require", "collectgarbage", "rawset", "rawget", "setmetatable"];

    /// <summary>
    /// Initialises a new instance of <see cref="LuaScriptExecutor"/>.
    /// </summary>
    /// <param name="options">Lua execution configuration.</param>
    /// <param name="logger">Structured diagnostic logger.</param>
    /// <param name="eventPublisher">Event publisher for Lua script events.</param>
    public LuaScriptExecutor(
        LuaExecutionOptions options,
        ILogger<LuaScriptExecutor> logger,
        IEventPublisher eventPublisher)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
    }

    /// <inheritdoc />
    public async Task<bool> ExecuteAsync(
        string script,
        TransformationContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(script);
        ArgumentNullException.ThrowIfNull(context);

        // Check if script is quarantined
        if (IsScriptQuarantined(context.ApiKeyId, context.ConsumerId, out var quarantineInfo))
        {
            _logger.LogWarning(
                "Skipping quarantined Lua script for key {ApiKeyId}, consumer {ConsumerId}. "
                +"Quarantine expires at {QuarantineUntil}, failures: {FailureCount}",
                context.ApiKeyId,
                context.ConsumerId,
                quarantineInfo.quarantineUntil,
                quarantineInfo.failureCount);

            await PublishQuarantinedEvent(
                context.ApiKeyId,
                context.ConsumerId,
                context.RuleId ?? "unknown",
                context.RuleName ?? "unknown",
                quarantineInfo.failureCount,
                _options.QuarantineThreshold);

            return true; // Skip execution but allow request to proceed
        }

        if (_options.MaxScriptSizeBytes > 0
            && System.Text.Encoding.UTF8.GetByteCount(script) > _options.MaxScriptSizeBytes)
        {
            throw new TransformationScriptException(
                $"Script exceeds the maximum allowed size of {_options.MaxScriptSizeBytes} bytes.");
        }

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(_options.MaxExecutionMs);

        var executionTimer = System.Diagnostics.Stopwatch.StartNew();
        long memoryUsed = 0;
        DynValue result;

        try
        {
            result = await Task.Run(
                () => RunScript(script, context, timeoutCts.Token, out memoryUsed),
                timeoutCts.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // Timeout, not caller-requested cancellation.
            await PublishFailedEvent(
                context.ApiKeyId,
                context.ConsumerId,
                context.RuleId ?? "unknown",
                context.RuleName ?? "unknown",
                "timeout",
                $"Script execution exceeded {_options.MaxExecutionMs} ms time limit",
                _options.MaxExecutionMs,
                memoryUsed);

            HandleScriptFailure(context.ApiKeyId, context.ConsumerId);
            throw new OperationCanceledException(
                $"Lua script execution exceeded the {_options.MaxExecutionMs} ms time limit.");
        }
        catch (ScriptRuntimeException ex)
        {
            await PublishFailedEvent(
                context.ApiKeyId,
                context.ConsumerId,
                context.RuleId ?? "unknown",
                context.RuleName ?? "unknown",
                "runtime_error",
                ex.DecoratedMessage,
                executionTimer.ElapsedMilliseconds,
                memoryUsed);

            HandleScriptFailure(context.ApiKeyId, context.ConsumerId);
            throw new TransformationScriptException(ex.DecoratedMessage, ex);
        }
        catch (SyntaxErrorException ex)
        {
            await PublishFailedEvent(
                context.ApiKeyId,
                context.ConsumerId,
                context.RuleId ?? "unknown",
                context.RuleName ?? "unknown",
                "syntax_error",
                ex.DecoratedMessage,
                executionTimer.ElapsedMilliseconds,
                memoryUsed);

            HandleScriptFailure(context.ApiKeyId, context.ConsumerId);
            throw new TransformationScriptException($"Lua syntax error: {ex.DecoratedMessage}", ex);
        }
        catch (LuaAllocationLimitExceededException ex)
        {
            await PublishFailedEvent(
                context.ApiKeyId,
                context.ConsumerId,
                context.RuleId ?? "unknown",
                context.RuleName ?? "unknown",
                "out_of_memory",
                $"Script exceeded memory limit of {_options.MaxMemoryBytes} bytes",
                executionTimer.ElapsedMilliseconds,
                memoryUsed);

            HandleScriptFailure(context.ApiKeyId, context.ConsumerId);
            throw new TransformationScriptException(
                $"Lua script exceeded memory limit of {_options.MaxMemoryBytes} bytes", ex);
        }
        catch (Exception ex) when (ex is not TransformationScriptException && !(ex is OutOfMemoryException || ex is SyntaxErrorException))
        {
            await PublishFailedEvent(
                context.ApiKeyId,
                context.ConsumerId,
                context.RuleId ?? "unknown",
                context.RuleName ?? "unknown",
                "unexpected_error",
                ex.Message,
                executionTimer.ElapsedMilliseconds,
                memoryUsed);

            HandleScriptFailure(context.ApiKeyId, context.ConsumerId);
            throw;
        }

        executionTimer.Stop();

        if (result.Type == DataType.String
            && string.Equals(result.String, "BLOCK", StringComparison.OrdinalIgnoreCase))
        {
            context.IsBlocked = true;

            _logger.LogInformation(
                "Lua script blocked request for consumer {ConsumerId} key {ApiKeyId}",
                context.ConsumerId, context.ApiKeyId);

            return false;
        }

        return true;
    }

    /// <inheritdoc />
    public ScriptValidationResult Validate(string script)
    {
        if (string.IsNullOrWhiteSpace(script))
        {
            return new ScriptValidationResult { IsValid = false, Errors = ["Script is empty."] };
        }

        var errors = new List<string>();

        // Size check.
        if (_options.MaxScriptSizeBytes > 0
            && System.Text.Encoding.UTF8.GetByteCount(script) > _options.MaxScriptSizeBytes)
        {
            errors.Add($"Script exceeds maximum size of {_options.MaxScriptSizeBytes} bytes.");
        }

        // Check for built-in blocked globals via simple string scan before attempting a parse.
        foreach (var blocked in _builtinBlockedGlobals)
        {
            if (script.Contains(blocked, StringComparison.Ordinal))
            {
                errors.Add($"Use of disallowed global '{blocked}' detected.");
            }
        }

        // Check for additionally configured disallowed globals.
        foreach (var blocked in _options.DisallowedGlobals)
        {
            if (script.Contains(blocked, StringComparison.Ordinal))
            {
                errors.Add($"Use of disallowed global '{blocked}' detected.");
            }
        }

        // Attempt a parse to catch syntax errors.
        try
        {
            var lua = BuildSandbox();
            lua.LoadString(script);
        }
        catch (SyntaxErrorException ex)
        {
            _logger.LogError(ex, "Lua script validation syntax error");
            errors.Add($"Syntax error: {ex.DecoratedMessage}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lua script validation error");
            errors.Add($"Validation error: {ex.Message}");
        }

        return new ScriptValidationResult { IsValid = errors.Count == 0, Errors = errors };
    }

    // -------------------------------------------------------------------------
    // Internal helpers
    // -------------------------------------------------------------------------

    private bool IsScriptQuarantined(string? apiKeyId, string? consumerId, out (int failureCount, DateTime quarantineUntil) quarantineInfo)
    {
        var key = GetQuarantineKey(apiKeyId, consumerId);
        lock (_quarantineRegistry)
        {
            if (_quarantineRegistry.TryGetValue(key, out var info))
            {
                if (DateTime.UtcNow < info.quarantineUntil)
                {
                    quarantineInfo = info;
                    return true;
                }
                // Quarantine expired, remove from registry
                _quarantineRegistry.Remove(key);
            }
        }
        quarantineInfo = (0, DateTime.MinValue);
        return false;
    }

    private void HandleScriptFailure(string? apiKeyId, string? consumerId)
    {
        if (_options.QuarantineThreshold <= 0)
        {
            // Quarantine disabled
            return;
        }

        var key = GetQuarantineKey(apiKeyId, consumerId);
        lock (_quarantineRegistry)
        {
            var currentCount = _quarantineRegistry.TryGetValue(key, out var existing) ? existing.failureCount : 0;
            var newCount = currentCount + 1;

            if (newCount >= _options.QuarantineThreshold)
            {
                // Quarantine the script
                var quarantineUntil = DateTime.UtcNow.AddSeconds(_options.QuarantineDurationSeconds);
                _quarantineRegistry[key] = (newCount, quarantineUntil);
                _logger.LogWarning(
                    "Lua script for key {ApiKeyId}, consumer {ConsumerId} quarantined for {DurationSeconds}s "
                    +"after {FailureCount} failures",
                    apiKeyId,
                    consumerId,
                    _options.QuarantineDurationSeconds,
                    newCount);
            }
            else
            {
                // Increment failure count
                _quarantineRegistry[key] = (newCount, DateTime.MinValue);
            }
        }
    }

    private async Task PublishFailedEvent(
        string? apiKeyId,
        string? consumerId,
        string scriptId,
        string scriptName,
        string failureType,
        string errorMessage,
        long executionTimeMs,
        long memoryUsedBytes)
    {
        try
        {
            var @event = new LuaScriptFailedEvent
            {
                ScriptId = scriptId,
                ScriptName = scriptName,
                ApiKeyId = apiKeyId,
                ConsumerId = consumerId,
                FailureType = failureType,
                ErrorType = failureType,
                ErrorMessage = errorMessage,
                ExecutionTimeMs = (int)executionTimeMs,
                MemoryUsedBytes = (int)memoryUsedBytes
            };

            await _eventPublisher.PublishAsync(@event);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish LuaScriptFailedEvent");
        }
    }

    private async Task PublishQuarantinedEvent(
        string? apiKeyId,
        string? consumerId,
        string scriptId,
        string scriptName,
        int consecutiveFailures,
        int quarantineThreshold)
    {
        try
        {
            var @event = new LuaScriptQuarantinedEvent
            {
                ScriptId = scriptId,
                ScriptName = scriptName,
                ApiKeyId = apiKeyId,
                ConsumerId = consumerId,
                ConsecutiveFailures = consecutiveFailures,
                QuarantineThreshold = quarantineThreshold,
                ErrorType = "quarantined",
                ErrorMessage = $"Script quarantined after {consecutiveFailures}/{quarantineThreshold} failures"
            };

            await _eventPublisher.PublishAsync(@event);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish LuaScriptQuarantinedEvent");
        }
    }

    private string GetQuarantineKey(string? apiKeyId, string? consumerId)
    {
        return $"{apiKeyId}:{consumerId}";
    }

    private DynValue RunScript(
        string source,
        TransformationContext context,
        CancellationToken cancellationToken,
        out long memoryUsed)
    {
        var lua = BuildSandbox();

        // Build the `request` global table that scripts read from and write to.
        var requestTable = new Table(lua);
        var headersTable = BuildTable(lua, context.Headers);
        var queryTable = BuildTable(lua, context.QueryParameters);

        requestTable["headers"] = headersTable;
        requestTable["query"] = queryTable;
        requestTable["body"] = context.Body ?? (object)DynValue.Nil;
        requestTable["method"] = context.Method;
        requestTable["path"] = context.Path;
        requestTable["ip"] = context.SourceIp;
        requestTable["consumer_id"] = context.ConsumerId ?? (object)DynValue.Nil;
        requestTable["api_key_id"] = context.ApiKeyId ?? (object)DynValue.Nil;

        lua.Globals["request"] = requestTable;

        // Compile before the guard is attached so parser allocations are not charged to the script.
        var chunk = lua.LoadString(source);

        var guard = new LuaExecutionGuard(cancellationToken, _options.MaxMemoryBytes);
        lua.AttachDebugger(guard);
        lua.DebuggerEnabled = true;

        DynValue result;
        try
        {
            result = lua.Call(chunk);
        }
        finally
        {
            memoryUsed = guard.AllocatedBytes;
        }

        // Write mutations back — headers.
        SyncTableToDict(headersTable, context.Headers);

        // Write mutations back — query parameters.
        SyncTableToDict(queryTable, context.QueryParameters);

        // Path and method may also have been overwritten.
        if (requestTable.Get("path") is { Type: DataType.String } pathVal)
        {
            context.Path = pathVal.String;
        }

        if (requestTable.Get("method") is { Type: DataType.String } methodVal)
        {
            context.Method = methodVal.String;
        }

        // If the script wrote a block_reason string, capture it.
        if (requestTable.Get("block_reason") is { Type: DataType.String } reasonVal)
        {
            context.BlockReason = reasonVal.String;
        }

        return result;
    }

    private Script BuildSandbox()
    {
        var lua = new Script(SandboxModules)
        {
            Options =
            {
                // Prevent scripts from loading external files.
                ScriptLoader = new InvalidScriptLoader()
            }
        };

        // Remove built-in dangerous globals.
        foreach (var g in _builtinBlockedGlobals)
        {
            lua.Globals[g] = DynValue.Nil;
        }

        // Remove additionally configured disallowed globals.
        foreach (var g in _options.DisallowedGlobals)
        {
            lua.Globals[g] = DynValue.Nil;
        }

        return lua;
    }

    private static Table BuildTable(Script lua, Dictionary<string, string> source)
    {
        var t = new Table(lua);
        foreach (var (k, v) in source)
        {
            t[k] = v;
        }

        return t;
    }

    private static void SyncTableToDict(Table source, Dictionary<string, string> target)
    {
        // Remove keys no longer present in the Lua table.
        var toRemove = target.Keys
            .Where(k => source.Get(k).IsNil())
            .ToList();

        foreach (var k in toRemove)
        {
            target.Remove(k);
        }

        // Add or update entries from the Lua table.
        foreach (var pair in source.Pairs)
        {
            if (pair.Key.Type == DataType.String
                && pair.Value.Type == DataType.String)
            {
                target[pair.Key.String] = pair.Value.String;
            }
        }
    }

    // Instruction-level hook. MoonSharp polls IsPauseRequested between Lua instructions, so throwing
    // from it unwinds the interpreter on the worker thread, including tight loops that never yield.
    private sealed class LuaExecutionGuard : IDebugger
    {
        // Allocation sampling interval in polls; the counter read is cheap but not free.
        private const int AllocationCheckInterval = 64;

        private readonly CancellationToken _cancellationToken;
        private readonly long _allocationBudget;
        private readonly long _startAllocatedBytes;
        private int _polls;

        public LuaExecutionGuard(CancellationToken cancellationToken, long allocationBudget)
        {
            _cancellationToken = cancellationToken;
            _allocationBudget = allocationBudget;
            _startAllocatedBytes = GC.GetAllocatedBytesForCurrentThread();
        }

        /// <summary>Gets the managed bytes allocated on the executing thread since the guard was created.</summary>
        public long AllocatedBytes => GC.GetAllocatedBytesForCurrentThread() - _startAllocatedBytes;

        public bool IsPauseRequested()
        {
            _cancellationToken.ThrowIfCancellationRequested();

            if (_allocationBudget > 0
                && ++_polls % AllocationCheckInterval == 0
                && AllocatedBytes > _allocationBudget)
            {
                throw new LuaAllocationLimitExceededException(AllocatedBytes);
            }

            return false;
        }

        public DebuggerAction GetAction(int ip, SourceRef sourceref) =>
            new() { Action = DebuggerAction.ActionType.Run };

        public DebuggerCaps GetDebuggerCaps() => DebuggerCaps.CanDebugSourceCode;

        public List<DynamicExpression> GetWatchItems() => [];

        public void RefreshBreakpoints(IEnumerable<SourceRef> refs) { }

        public void SetByteCode(string[] byteCode) { }

        public void SetDebugService(DebugService debugService) { }

        public void SetSourceCode(SourceCode sourceCode) { }

        public void SignalExecutionEnded() { }

        public bool SignalRuntimeException(ScriptRuntimeException ex) => false;

        public void Update(WatchType watchType, IEnumerable<WatchItem> items) { }
    }

    // Raised by LuaExecutionGuard when a script's allocation budget is exhausted.
    private sealed class LuaAllocationLimitExceededException(long allocatedBytes) : Exception(
        $"Lua script allocated {allocatedBytes} bytes, exceeding its memory limit.")
    {
        public long AllocatedBytes { get; } = allocatedBytes;
    }

    // Prevents scripts from using `dofile`, `loadfile`, or `require` to load external Lua files.
    private sealed class InvalidScriptLoader : ScriptLoaderBase
    {
        public override object LoadFile(string file, Table globalContext) =>
            throw new ScriptRuntimeException($"Loading external scripts is not permitted (attempted: '{file}').");

        public override bool ScriptFileExists(string name) => false;
    }
}

/// <summary>
/// Raised when the Lua runtime encounters an unhandled error during script execution,
/// or when a script violates a static security constraint.
/// </summary>
public sealed class TransformationScriptException : Exception
{
    /// <summary>Initialises a new instance with a message.</summary>
    public TransformationScriptException(string message) : base(message) { }

    /// <summary>Initialises a new instance with a message and an inner exception.</summary>
    public TransformationScriptException(string message, Exception inner) : base(message, inner) { }
}
