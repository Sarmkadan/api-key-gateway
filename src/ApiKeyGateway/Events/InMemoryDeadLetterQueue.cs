// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;

namespace ApiKeyGateway.Events;

/// <summary>
/// Thread-safe in-memory bounded dead-letter queue for storing failed events.
/// Implements FIFO (First-In-First-Out) semantics with configurable maximum size.
/// When the queue is full, oldest entries are automatically removed to make room for new ones.
/// </summary>
public sealed class InMemoryDeadLetterQueue : IDeadLetterQueue
{
    private readonly ConcurrentQueue<DeadLetterEntry> _queue = new();
    private readonly int _maxSize;
    private readonly object _syncLock = new();

    /// <summary>
    /// Gets the number of entries currently in the dead-letter queue.
    /// </summary>
    public int Count => _queue.Count;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryDeadLetterQueue"/> class.
    /// </summary>
    /// <param name="maxSize">Maximum number of entries to keep in the queue. Use 0 for unbounded.</param>
    public InMemoryDeadLetterQueue(int maxSize = 1000)
    {
        _maxSize = maxSize < 0 ? 0 : maxSize;
    }

    /// <summary>
    /// Adds an entry to the dead-letter queue.
    /// If the queue is full, the oldest entry is removed to make room.
    /// </summary>
    /// <param name="entry">The dead-letter entry to add.</param>
    public void Add(DeadLetterEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        lock (_syncLock)
        {
            if (_maxSize > 0 && _queue.Count >= _maxSize)
            {
                // Remove oldest entry to make room
                _ = _queue.TryDequeue(out _);
            }

            _queue.Enqueue(entry);
        }
    }

    /// <summary>
    /// Gets all entries in the dead-letter queue without removing them.
    /// </summary>
    /// <returns>Read-only list of all dead-letter entries.</returns>
    public IReadOnlyList<DeadLetterEntry> GetAll()
    {
        lock (_syncLock)
        {
            return _queue.ToList().AsReadOnly();
        }
    }

    /// <summary>
    /// Clears all entries from the dead-letter queue.
    /// </summary>
    public void Clear()
    {
        lock (_syncLock)
        {
            while (_queue.TryDequeue(out _))
            {
                // Drain the queue
            }
        }
    }

    /// <summary>
    /// Removes and returns the oldest entry from the dead-letter queue.
    /// </summary>
    /// <returns>The oldest dead-letter entry, or null if the queue is empty.</returns>
    public DeadLetterEntry? Take()
    {
        lock (_syncLock)
        {
            return _queue.TryDequeue(out var entry) ? entry : null;
        }
    }

    /// <summary>
    /// Gets the oldest entry from the dead-letter queue without removing it.
    /// </summary>
    /// <returns>The oldest dead-letter entry, or null if the queue is empty.</returns>
    public DeadLetterEntry? Peek()
    {
        lock (_syncLock)
        {
            return _queue.TryPeek(out var entry) ? entry : null;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the dead-letter queue is empty.
    /// </summary>
    public bool IsEmpty => _queue.IsEmpty;
}