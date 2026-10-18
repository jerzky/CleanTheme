using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using Microsoft.Extensions.Logging;

namespace CleanTheme.Logging;

/// <summary>
/// Bounded in-app log with source, category, level and text filters. <see cref="Visible"/> is what a <see cref="LogView"/> shows.
/// </summary>
/// <remarks>
/// <see cref="Post"/> is safe from any thread: entries queue up and are added on the feed's dispatcher in batches.
/// Everything else (collections, filters, <see cref="Add"/>, <see cref="Clear"/>, <see cref="PropertyChanged"/>) is
/// dispatcher-thread only.
/// </remarks>
public sealed class LogFeed : INotifyPropertyChanged
{
    private readonly Dispatcher _dispatcher;
    private readonly LinkedList<LogEntry> _all = new();
    private readonly ObservableCollection<LogEntry> _visible = [];
    private readonly ObservableCollection<string> _sources = [];
    private readonly HashSet<string> _knownSources = new(StringComparer.Ordinal);
    private readonly ObservableCollection<string> _categories = [];
    private readonly HashSet<string> _knownCategories = new(StringComparer.Ordinal);
    private readonly ConcurrentQueue<LogEntry> _pending = new();
    private int _drainScheduled;
    private string? _sourceFilter;
    private string? _categoryFilter;
    private LogLevel _minimumLevel = LogLevel.Trace;
    private string? _textFilter;

    /// <param name="dispatcher">The UI dispatcher that owns <see cref="Visible"/>.</param>
    /// <param name="capacity">Lines kept; the oldest are dropped beyond this.</param>
    public LogFeed(Dispatcher dispatcher, int capacity = 2000)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);
        ArgumentOutOfRangeException.ThrowIfLessThan(capacity, 1);
        _dispatcher = dispatcher;
        Capacity = capacity;
        Visible = new ReadOnlyObservableCollection<LogEntry>(_visible);
        Sources = new ReadOnlyObservableCollection<string>(_sources);
        Categories = new ReadOnlyObservableCollection<string>(_categories);
    }

    /// <summary>Raised on the dispatcher thread when a filter, <see cref="TotalCount"/> or <see cref="IsFiltered"/> changes.</summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>A feed owned by the calling thread's dispatcher (call on the UI thread).</summary>
    public static LogFeed CreateForCurrentThread(int capacity = 2000) => new(Dispatcher.CurrentDispatcher, capacity);

    /// <summary>The lines that pass the filters, oldest first.</summary>
    public ReadOnlyObservableCollection<LogEntry> Visible { get; }

    /// <summary>Every non-null <see cref="LogEntry.Source"/> seen since the last <see cref="Clear"/>, in first-seen order.</summary>
    public ReadOnlyObservableCollection<string> Sources { get; }

    /// <summary>Every non-null <see cref="LogEntry.Category"/> seen since the last <see cref="Clear"/>, in first-seen order.</summary>
    public ReadOnlyObservableCollection<string> Categories { get; }

    /// <summary>Lines kept before the oldest are dropped.</summary>
    public int Capacity { get; }

    /// <summary>Lines kept, visible or not.</summary>
    public int TotalCount => _all.Count;

    /// <summary>Whether any filter hides lines.</summary>
    public bool IsFiltered => _sourceFilter != null || _categoryFilter != null || _minimumLevel > LogLevel.Trace || !string.IsNullOrEmpty(_textFilter);

    /// <summary>Only lines with this <see cref="LogEntry.Source"/> are visible; null shows every source.</summary>
    public string? SourceFilter
    {
        get => _sourceFilter;
        set => SetFilter(ref _sourceFilter, value);
    }

    /// <summary>Only lines with this <see cref="LogEntry.Category"/> are visible; null shows every category.</summary>
    public string? CategoryFilter
    {
        get => _categoryFilter;
        set => SetFilter(ref _categoryFilter, value);
    }

    /// <summary>Lines below this level are hidden (still kept).</summary>
    public LogLevel MinimumLevel
    {
        get => _minimumLevel;
        set => SetFilter(ref _minimumLevel, value);
    }

    /// <summary>Only lines whose message contains this text (case-insensitive) are visible; null or empty shows all.</summary>
    public string? TextFilter
    {
        get => _textFilter;
        set => SetFilter(ref _textFilter, value);
    }

    /// <summary>Removes every filter.</summary>
    public void ResetFilters()
    {
        _dispatcher.VerifyAccess();
        if (!IsFiltered)
        {
            return;
        }
        _sourceFilter = null;
        _categoryFilter = null;
        _minimumLevel = LogLevel.Trace;
        _textFilter = null;
        Refilter();
        OnPropertyChanged(nameof(SourceFilter));
        OnPropertyChanged(nameof(CategoryFilter));
        OnPropertyChanged(nameof(MinimumLevel));
        OnPropertyChanged(nameof(TextFilter));
        OnPropertyChanged(nameof(IsFiltered));
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
        AddCore(entry);
        OnPropertyChanged(nameof(TotalCount));
    }

    /// <summary>Removes every line, source and category, including queued lines. Filters stay.</summary>
    public void Clear()
    {
        _dispatcher.VerifyAccess();
        _pending.Clear();
        _all.Clear();
        _visible.Clear();
        _sources.Clear();
        _knownSources.Clear();
        _categories.Clear();
        _knownCategories.Clear();
        OnPropertyChanged(nameof(TotalCount));
    }

    private void AddCore(LogEntry entry)
    {
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
        if (entry.Source != null && _knownSources.Add(entry.Source))
        {
            _sources.Add(entry.Source);
        }
        if (entry.Category != null && _knownCategories.Add(entry.Category))
        {
            _categories.Add(entry.Category);
        }
        if (Matches(entry))
        {
            _visible.Add(entry);
        }
    }

    private void Drain()
    {
        // Reset first: a Post racing with this drain schedules another one rather than being lost.
        Volatile.Write(ref _drainScheduled, 0);
        var added = false;
        while (_pending.TryDequeue(out var entry))
        {
            AddCore(entry);
            added = true;
        }
        if (added)
        {
            OnPropertyChanged(nameof(TotalCount));
        }
    }

    private void SetFilter<T>(ref T field, T value, [CallerMemberName] string name = "")
    {
        _dispatcher.VerifyAccess();
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }
        var wasFiltered = IsFiltered;
        field = value;
        Refilter();
        OnPropertyChanged(name);
        if (wasFiltered != IsFiltered)
        {
            OnPropertyChanged(nameof(IsFiltered));
        }
    }

    private void Refilter()
    {
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
        entry.Level >= _minimumLevel
        && (_sourceFilter == null || entry.Source == _sourceFilter)
        && (_categoryFilter == null || entry.Category == _categoryFilter)
        && (string.IsNullOrEmpty(_textFilter) || entry.Message.Contains(_textFilter, StringComparison.OrdinalIgnoreCase));

    private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
