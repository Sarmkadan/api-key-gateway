// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Events;

/// <summary>
/// Base class for Lua script execution events.
/// Fired whenever a Lua script is executed, succeeds, fails, or is quarantined.
/// </summary>
public abstract record LuaScriptEvent
{
    /// <summary>Unique identifier of this event</summary>
    public Guid EventId { get; } = Guid.NewGuid();

    /// <summary>When the event was created (UTC)</summary>
    public DateTime Timestamp { get; } = DateTime.UtcNow;

    /// <summary>Identifier of the Lua script involved</summary>
    public string ScriptId { get; set; } = null!;

    /// <summary>Display name of the Lua script involved</summary>
    public string ScriptName { get; set; } = null!;

    /// <summary>ID of the API key that triggered the execution, if any</summary>
    public string? ApiKeyId { get; set; }

    /// <summary>ID of the consumer that triggered the execution, if any</summary>
    public string? ConsumerId { get; set; }

    /// <summary>Category of the error</summary>
    public string ErrorType { get; set; } = null!;

    /// <summary>Description of the error</summary>
    public string ErrorMessage { get; set; } = null!;
}

/// <summary>
/// Fired when a Lua script execution fails due to timeout or runtime error.
/// </summary>
public record LuaScriptFailedEvent : LuaScriptEvent
{
    /// <summary>Specific kind of failure (e.g. timeout, runtime error)</summary>
    public string FailureType { get; set; } = null!;

    /// <summary>Time the script ran before failing, in milliseconds</summary>
    public int ExecutionTimeMs { get; set; }

    /// <summary>Memory consumed by the script before failing, in bytes</summary>
    public long MemoryUsedBytes { get; set; }
}

/// <summary>
/// Fired when a Lua script is quarantined due to repeated failures.
/// The script will be skipped until an operator re-enables it.
/// </summary>
public record LuaScriptQuarantinedEvent : LuaScriptEvent
{
    /// <summary>Number of consecutive failures that led to quarantine</summary>
    public int ConsecutiveFailures { get; set; }

    /// <summary>Failure count at which a script is quarantined</summary>
    public int QuarantineThreshold { get; set; }
}

/// <summary>
/// Fired when a previously quarantined Lua script is re-enabled by an operator.
/// </summary>
public record LuaScriptReenabledEvent : LuaScriptEvent
{
    /// <summary>Reason the script was re-enabled</summary>
    public string Reason { get; set; } = "Operator re-enabled after quarantine";
}