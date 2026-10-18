using System.Windows;
using System.Windows.Threading;
using CleanTheme.Logging;
using CleanTheme.Wpf.Controls;
using Microsoft.Extensions.Logging;

namespace CleanTheme.Gallery;

public partial class MainWindow : Window
{
    private static readonly string[] SampleMessages =
    [
        "Tick 18.4 ms",
        "Client connected from 127.0.0.1:50211",
        "Spawned ALPHA-1 (UAV)",
        "Mission plan sent to BLUE operators",
        "Snapshot published: 42 entities",
    ];

    // Null is the host itself; the others stand in for child processes.
    private static readonly string?[] SampleSources = [null, "ALPHA-1", null, "BRAVO-2", "CHARLIE-3", null];

    private readonly LogFeed _feed = LogFeed.CreateForCurrentThread();
    private readonly DispatcherTimer _ticker = new() { Interval = TimeSpan.FromMilliseconds(700) };
    private int _tick;

    public MainWindow()
    {
        InitializeComponent();
        Rows.ItemsSource = new[]
        {
            new SampleRow("ALPHA-1", StatusState.Positive, 12, 412.5),
            new SampleRow("BRAVO-2", StatusState.Warning, 3, 0),
            new SampleRow("CHARLIE-3", StatusState.Negative, 27, 1288.25),
        };
        LongList.ItemsSource = Enumerable.Range(1, 60)
            .Select(i => $"Item {i:00}: a line long enough to need the horizontal scroll bar as well as the vertical one")
            .ToArray();

        Log.Feed = _feed;
        _ticker.Tick += (_, _) => WriteSampleLine();
        _ticker.Start();
        Closed += (_, _) => _ticker.Stop();
    }

    private void WriteSampleLine()
    {
        var level = (_tick % 7) switch
        {
            0 => LogLevel.Debug,
            5 => LogLevel.Warning,
            _ => LogLevel.Information,
        };
        // Post works from any thread; the timer happens to run on the UI thread.
        _feed.Post(new LogEntry(DateTime.UtcNow, level, SampleSources[_tick % SampleSources.Length], SampleMessages[_tick % SampleMessages.Length]));
        _tick++;
    }

    private void OnLogError(object sender, RoutedEventArgs e) =>
        _feed.Post(new LogEntry(DateTime.UtcNow, LogLevel.Error, "BRAVO-2", "Node BRAVO-2 exited with code 1"));

    private void OnClearLog(object sender, RoutedEventArgs e) => _feed.Clear();

    private sealed record SampleRow(string Name, StatusState State, int Count, double Value);
}
