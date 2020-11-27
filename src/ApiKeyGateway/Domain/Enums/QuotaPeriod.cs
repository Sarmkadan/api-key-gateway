// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Domain.Enums;

/// <summary>
/// Defines the calendar-based reset period for a usage quota
/// </summary>
public enum QuotaPeriod
{
    /// <summary>
    /// Quota resets every calendar day (midnight UTC)
    /// </summary>
    Daily = 1,

    /// <summary>
    /// Quota resets on the first day of every calendar month (midnight UTC)
    /// </summary>
    Monthly = 2,

    /// <summary>
    /// Quota resets every calendar hour (top of the hour, UTC)
    /// </summary>
    Hour = 3,

    /// <summary>
    /// Quota resets every calendar day (midnight UTC)
    /// </summary>
    Day = 4,

    /// <summary>
    /// Quota resets every calendar week (Sunday midnight UTC)
    /// </summary>
    Week = 5,

    /// <summary>
    /// Quota resets on the first day of every calendar month (midnight UTC)
    /// </summary>
    Month = 6
}
