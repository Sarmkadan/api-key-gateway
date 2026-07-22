// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Events;

/// <summary>
/// Interface for a dead-letter queue that stores failed events.
/// Implementations can be in-memory, file-based, database-backed, etc.
/// </summary>
public interface IDeadLetterQueue
{
    /// <summary>
    /// Gets the number of entries currently in the dead-letter queue.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Adds an entry to the dead-letter queue.
    /// </summary>
    /// <param name="entry">The dead-letter entry to add.</param>
    void Add(DeadLetterEntry entry);

    /// <summary>
    /// Gets all entries in the dead-letter queue without removing them.
    /// </summary>
    /// <returns>Read-only list of all dead-letter entries.</returns>
    IReadOnlyList<DeadLetterEntry> GetAll();

    /// <summary>
    /// Clears all entries from the dead-letter queue.
    /// </summary>
    void Clear();

    /// <summary>
    /// Removes and returns the oldest entry from the dead-letter queue.
    /// </summary>
    /// <returns>The oldest dead-letter entry, or null if the queue is empty.</returns>
    DeadLetterEntry? Take();

    /// <summary>
    /// Gets the oldest entry from the dead-letter queue without removing it.
    /// </summary>
    /// <returns>The oldest dead-letter entry, or null if the queue is empty.</returns>
    DeadLetterEntry? Peek();

    /// <summary>
    /// Gets a value indicating whether the dead-letter queue is empty.
    /// </summary>
    bool IsEmpty { get; }
}