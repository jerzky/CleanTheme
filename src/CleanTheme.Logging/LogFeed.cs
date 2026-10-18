using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using Microsoft.Extensions.Logging;

namespace CleanTheme.Logging;

/// <summary>
/// Bounded in-app log with source and level filters. <see cref="Visible"/> is what a <see cref="LogView"/> shows.
/// </summary>
/// <remarks>
/// <see cref="Post"/> is safe from any thread: entries queue up and are added on the feed's dispatcher in batches.
/// Everything else (<see cref="Visible"/>, the filters, <see cref="Add"/>, <see cref="Clear"/>) is dispatcher-thread only.
/// </remarks>
public sealed class LogFeed
{
    private readonly Dispatcher _dispatcher;
    private readonly LinkedList<LogEntry> _all = new();
    private readonly ObservableCollection<LogEntry> _visible = [];
    private readonly ConcurrentQueue<LogEntry> _pending = new();
    private int _drainScheduled;
    private string? _sourceFilter;
    private LogLevel _minimumLevel = LogLevel.Trace;

    /// <param name="dispatcher">The UI dispatcher that owns <see cref="Visible"/>.</param>
    /// <param name="capacity">Lines kept; the oldest are dropped beyond this.</param>
    public LogFeed(Dispatcher dispatcher, int capacity = 2000)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);
        ArgumentOutOfRangeException.ThrowIfLessThan(capacity, 1);
        _dispatcher = dispatcher;
        Capacity = capacity;
        Visible = new ReadOnlyObservableCollection<LogEntry>(_visible);
    }

    /// <summary>A feed owned by the calling thread's dispatcher (call on the UI thread).</summary>
    public static LogFeed CreateForCurrentThread(int capacity = 2000) => new(Dispatcher.CurrentDispatcher, capacity);

    /// <summary>The lines that pass the filters, oldest first.</summary>
    public ReadOnlyObservableCollection<LogEntry> Visible { get; }

    /// <summary>Lines kept before the oldest are dropped.</summary>
    public int Capacity { get; }

    /// <summary>Only lines with this <see cref="LogEntry.Source"/> are visible; null shows every source.</summary>
    public string? SourceFilter
    {
        get => _sourceFilter;
        set
        {
            if (_sourceFilter == value)
            {
                return;
            }
            _sourceFilter = value;
            Refilter();
        }
    }

    /// <summary>Lines below this level are hidden (still kept).</summary>
    public LogLevel MinimumLevel
    {
        get => _minimumLevel;
        set
        {
            if (_minimumLevel == value)
            {
                return;
            }
            _minimumLevel = value;
            Refilter();
        }
    }

    /// <summary>Queues a line from any thread.</summary>
    public void Post(LogEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        _pending.Enqueue(entry);
        if (Interlocked.Exchange(ref _drainScheduled, 1) == 0)
        {
            _dispatcher.BeginInvoke(DispatcherPriority.Background, Drain);
        }
    }

    /// <summary>Adds a line. Dispatcher thread only; use <see cref="Post"/> from elsewhere.</summary>
    public void Add(LogEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        _dispatcher.VerifyAccess();
        _all.AddLast(entry);
        if (_all.Count > Capacity)
        {
            var dropped = _all.First!.Value; // Count > Capacity >= 1, so the list is not empty.
            _all.RemoveFirst();
            if (_visible.Count > 0 && ReferenceEquals(_visible[0], dropped))
            {
                _visible.RemoveAt(0);
            }
        }
        if (Matches(entry))
        {
            _visible.Add(entry);
        }
    }

    /// <summary>Removes every line, including queued ones.</summary>
    public void Clear()
    {
        _dispatcher.VerifyAccess();
        _pending.Clear();
        _all.Clear();
        _visible.Clear();
    }

    private void Drain()
    {
        // Reset first: a Post racing with this drain schedules another one rather than being lost.
        Volatile.Write(ref _drainScheduled, 0);
        while (_pending.TryDequeue(out var entry))
        {
            Add(entry);
        }
    }

    private void Refilter()
    {
        _dispatcher.VerifyAccess();
        _visible.Clear();
        foreach (var entry in _all)
        {
            if (Matches(entry))
            {
                _visible.Add(entry);
            }
        }
    }

    private bool Matches(LogEntry entry) =>
        entry.Level >= _minimumLevel && (_sourceFilter == null || entry.Source == _sourceFilter);
}
