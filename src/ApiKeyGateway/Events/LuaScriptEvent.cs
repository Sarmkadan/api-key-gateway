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
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime Timestamp { get; } = DateTime.UtcNow;
    public string ScriptId { get; set; } = null!;
    public string ScriptName { get; set; } = null!;
    public string ApiKeyId { get; set; } = null!;
    public string ConsumerId { get; set; } = null!;
    public string ErrorType { get; set; } = null!;
    public string ErrorMessage { get; set; } = null!;
}

/// <summary>
/// Fired when a Lua script execution fails due to timeout or runtime error.
/// </summary>
public record LuaScriptFailedEvent : LuaScriptEvent
{
    public string FailureType { get; set; } = null!;
    public int ExecutionTimeMs { get; set; }
    public long MemoryUsedBytes { get; set; }
}

/// <summary>
/// Fired when a Lua script is quarantined due to repeated failures.
/// The script will be skipped until an operator re-enables it.
/// </summary>
public record LuaScriptQuarantinedEvent : LuaScriptEvent
{
    public int ConsecutiveFailures { get; set; }
    public int QuarantineThreshold { get; set; }
}

/// <summary>
/// Fired when a previously quarantined Lua script is re-enabled by an operator.
/// </summary>
public record LuaScriptReenabledEvent : LuaScriptEvent
{
    public string Reason { get; set; } = "Operator re-enabled after quarantine";
}