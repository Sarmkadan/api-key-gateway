using System;
using System.Collections.Generic;
using System.Linq;

namespace ApiKeyGateway.Events;

/// <summary>
/// Extension methods for <see cref="UsageEvent"/> collections.
/// </summary>
public static class UsageEventExtensions
{
    /// <summary>
    /// Returns the total number of usage events in the sequence.
    /// </summary>
    public static int TotalCount(this IEnumerable<UsageEvent> events)
    {
        return events.Count();
    }

    /// <summary>
    /// Groups usage events by their <see cref="UsageEvent.ApiKeyId"/> property.
    /// </summary>
    public static IEnumerable<IGrouping<string, UsageEvent>> GroupByApiKey(this IEnumerable<UsageEvent> events)
    {
        return events.GroupBy(e => e.ApiKeyId);
    }

    /// <summary>
    /// Filters usage events to those whose <see cref="UsageEvent.Timestamp"/> falls within the specified period.
    /// </summary>
    public static IEnumerable<UsageEvent> FilterByPeriod(this IEnumerable<UsageEvent> events, DateTime from, DateTime to)
    {
        return events.Where(e => e.Timestamp >= from && e.Timestamp <= to);
    }
}
