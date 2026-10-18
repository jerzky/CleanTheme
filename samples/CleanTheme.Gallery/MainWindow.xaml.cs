using System.Windows;
using CleanTheme.Logging;
using CleanTheme.Wpf.Controls;
using Microsoft.Extensions.Logging;

namespace CleanTheme.Gallery;

public partial class MainWindow : Window
{
    private readonly LogFeed _feed;
    private readonly ILogger<MainWindow> _logger;

    public MainWindow(LogFeed feed, ILogger<MainWindow> logger)
    {
        _feed = feed;
        _logger = logger;
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
        LogWindowOpened(_logger);
    }

    private void OnLogError(object sender, RoutedEventArgs e)
    {
        try
        {
            throw new InvalidOperationException("Process exited with code 1");
        }
        catch (InvalidOperationException ex)
        {
            LogNodeFailed(_logger, ex, "BRAVO-2");
        }
    }

    private void OnClearLog(object sender, RoutedEventArgs e) => _feed.Clear();

    [LoggerMessage(Level = LogLevel.Information, Message = "Gallery window opened")]
    private static partial void LogWindowOpened(ILogger logger);

    [LoggerMessage(Level = LogLevel.Error, Message = "Node {Callsign} failed")]
    private static partial void LogNodeFailed(ILogger logger, Exception exception, string callsign);

    private sealed record SampleRow(string Name, StatusState State, int Count, double Value);
}
