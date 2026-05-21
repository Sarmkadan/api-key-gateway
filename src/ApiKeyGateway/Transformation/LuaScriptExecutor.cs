// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Configuration;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;

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
/// Script execution is delegated to a <see cref="Task.Run"/> worker thread so the calling
/// async context is not blocked. A linked <see cref="CancellationTokenSource"/> enforces the
/// wall-clock timeout; if the script exceeds the limit the task is abandoned and a timeout
/// error is recorded — note that a tight CPU-bound loop inside Lua cannot be pre-empted by
/// cancellation alone, so script authors should keep operations O(1) or O(n) with small n.
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

    // Globals that are always stripped from the sandbox environment.
    private static readonly string[] _blockedGlobals =
        ["dofile", "loadfile", "load", "require", "collectgarbage", "rawset", "rawget", "setmetatable"];

    /// <summary>
    /// Initialises a new instance of <see cref="LuaScriptExecutor"/>.
    /// </summary>
    /// <param name="options">Lua execution configuration.</param>
    /// <param name="logger">Structured diagnostic logger.</param>
    public LuaScriptExecutor(LuaExecutionOptions options, ILogger<LuaScriptExecutor> logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger  = logger  ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<bool> ExecuteAsync(
        string script,
        TransformationContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(script);
        ArgumentNullException.ThrowIfNull(context);

        if (_options.MaxScriptSizeBytes > 0
            && System.Text.Encoding.UTF8.GetByteCount(script) > _options.MaxScriptSizeBytes)
        {
            throw new TransformationScriptException(
                $"Script exceeds the maximum allowed size of {_options.MaxScriptSizeBytes} bytes.");
        }

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(_options.MaxExecutionMs);

        DynValue result;

        try
        {
            result = await Task.Run(() => RunScript(script, context), timeoutCts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // Timeout, not caller-requested cancellation.
            throw new OperationCanceledException(
                $"Lua script execution exceeded the {_options.MaxExecutionMs} ms time limit.");
        }
        catch (ScriptRuntimeException ex)
        {
            throw new TransformationScriptException(ex.DecoratedMessage, ex);
        }
        catch (SyntaxErrorException ex)
        {
            throw new TransformationScriptException($"Lua syntax error: {ex.DecoratedMessage}", ex);
        }

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
            return new ScriptValidationResult { IsValid = false, Errors = ["Script is empty."] };

        var errors = new List<string>();

        // Size check.
        if (_options.MaxScriptSizeBytes > 0
            && System.Text.Encoding.UTF8.GetByteCount(script) > _options.MaxScriptSizeBytes)
        {
            errors.Add($"Script exceeds maximum size of {_options.MaxScriptSizeBytes} bytes.");
        }

        // Check for blocked globals via simple string scan before attempting a parse.
        foreach (var blocked in _blockedGlobals)
        {
            if (script.Contains(blocked, StringComparison.Ordinal))
                errors.Add($"Use of disallowed global '{blocked}' detected.");
        }

        // Attempt a parse to catch syntax errors.
        try
        {
            var lua = BuildSandbox();
            lua.LoadString(script);
        }
        catch (SyntaxErrorException ex)
        {
            errors.Add($"Syntax error: {ex.DecoratedMessage}");
        }
        catch (Exception ex)
        {
            errors.Add($"Validation error: {ex.Message}");
        }

        return new ScriptValidationResult { IsValid = errors.Count == 0, Errors = errors };
    }

    // -------------------------------------------------------------------------
    // Internal helpers
    // -------------------------------------------------------------------------

    private DynValue RunScript(string source, TransformationContext context)
    {
        var lua = BuildSandbox();

        // Build the `request` global table that scripts read from and write to.
        var requestTable = new Table(lua);
        var headersTable = BuildTable(lua, context.Headers);
        var queryTable   = BuildTable(lua, context.QueryParameters);

        requestTable["headers"]     = headersTable;
        requestTable["query"]       = queryTable;
        requestTable["body"]        = context.Body ?? (object)DynValue.Nil;
        requestTable["method"]      = context.Method;
        requestTable["path"]        = context.Path;
        requestTable["ip"]          = context.SourceIp;
        requestTable["consumer_id"] = context.ConsumerId ?? (object)DynValue.Nil;
        requestTable["api_key_id"]  = context.ApiKeyId   ?? (object)DynValue.Nil;

        lua.Globals["request"] = requestTable;

        var result = lua.DoString(source);

        // Write mutations back — headers.
        SyncTableToDict(headersTable, context.Headers);

        // Write mutations back — query parameters.
        SyncTableToDict(queryTable, context.QueryParameters);

        // Path and method may also have been overwritten.
        if (requestTable.Get("path") is { Type: DataType.String } pathVal)
            context.Path = pathVal.String;

        if (requestTable.Get("method") is { Type: DataType.String } methodVal)
            context.Method = methodVal.String;

        // If the script wrote a block_reason string, capture it.
        if (requestTable.Get("block_reason") is { Type: DataType.String } reasonVal)
            context.BlockReason = reasonVal.String;

        return result;
    }

    private static Script BuildSandbox()
    {
        var lua = new Script(SandboxModules)
        {
            Options =
            {
                // Prevent scripts from loading external files.
                ScriptLoader = new InvalidScriptLoader()
            }
        };

        // Remove any remaining dangerous globals.
        foreach (var g in _blockedGlobals)
            lua.Globals[g] = DynValue.Nil;

        return lua;
    }

    private static Table BuildTable(Script lua, Dictionary<string, string> source)
    {
        var t = new Table(lua);
        foreach (var (k, v) in source) t[k] = v;
        return t;
    }

    private static void SyncTableToDict(Table source, Dictionary<string, string> target)
    {
        // Remove keys no longer present in the Lua table.
        var toRemove = target.Keys
            .Where(k => source.Get(k).IsNil())
            .ToList();

        foreach (var k in toRemove) target.Remove(k);

        // Add or update entries from the Lua table.
        foreach (var pair in source.Pairs)
        {
            if (pair.Key.Type   == DataType.String
             && pair.Value.Type == DataType.String)
            {
                target[pair.Key.String] = pair.Value.String;
            }
        }
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
