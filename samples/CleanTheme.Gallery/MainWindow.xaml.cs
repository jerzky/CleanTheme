using System.Windows;
using System.Windows.Controls;
using CleanTheme.Logging;
using CleanTheme.Wpf;
using CleanTheme.Wpf.Controls;
using Microsoft.Extensions.Logging;

namespace CleanTheme.Gallery;

public partial class MainWindow : CustomWindow
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
        StartDate.SelectedDate = DateTime.Today;

        ThemeChoice.SelectedIndex = (int)ThemeManager.Mode; // Light, Dark, System, in enum order.

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

    private void OnThemeChoiceChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ThemeChoice.SelectedIndex >= 0)
        {
            ThemeManager.Mode = (CleanThemeMode)ThemeChoice.SelectedIndex;
            LogThemeChanged(_logger, ThemeManager.Mode, ThemeManager.ActualMode);
        }
    }

    private void OnShowToast(object sender, RoutedEventArgs e)
    {
        var state = Enum.Parse<StatusState>((string)((Button)sender).Tag);
        var message = state switch
        {
            StatusState.Positive => "Scenario saved as baltic-cap.json",
            StatusState.Warning => "BRAVO-2 is low on fuel",
            StatusState.Negative => "CHARLIE-3 exited with code 1",
            _ => "3 units spawned",
        };
        Toasts.Show(message, state);
    }

    private void OnMessage(object sender, RoutedEventArgs e) =>
        CleanMessageBox.Show(this, "The scenario was saved to the Scenarios folder.", "Scenario", "Saved");

    private void OnConfirmDelete(object sender, RoutedEventArgs e)
    {
        var confirmed = CleanMessageBox.Confirm(this, "This removes the file from disk. Running units are not affected.",
            "Delete scenario", "Delete", heading: "Delete baltic-cap.json?", isDanger: true);
        Toasts.Show(confirmed ? "Scenario deleted" : "Kept the scenario", confirmed ? StatusState.Positive : StatusState.Neutral);
    }

    private void OnPrompt(object sender, RoutedEventArgs e)
    {
        var name = CleanMessageBox.Prompt(this, "Save scenario", "Name for a scenario of the 3 running units", "New scenario", "Save");
        if (name != null)
        {
            Toasts.Show($"Saved \"{name}\"", StatusState.Positive);
        }
    }

    private void OnNewScenario(object sender, RoutedEventArgs e) => Toasts.Show("New scenario (just a sample)");

    private void OnAbout(object sender, RoutedEventArgs e) =>
        CleanMessageBox.Show(this, "Every CleanTheme style and control on one page.", "About", "CleanTheme Gallery");

    private void OnOpenGitHub(object sender, RoutedEventArgs e)
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("https://github.com/jerzky/CleanTheme") { UseShellExecute = true });
        }
        catch { }
    }

    private void OnOpenSettings(object sender, RoutedEventArgs e) => NavRail.SelectedIndex = 4;

    private void OnExit(object sender, RoutedEventArgs e) => Close();

    [LoggerMessage(Level = LogLevel.Information, Message = "Gallery window opened")]
    private static partial void LogWindowOpened(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Theme set to {Mode} ({ActualMode})")]
    private static partial void LogThemeChanged(ILogger logger, CleanThemeMode mode, CleanThemeMode actualMode);

    [LoggerMessage(Level = LogLevel.Error, Message = "Node {Callsign} failed")]
    private static partial void LogNodeFailed(ILogger logger, Exception exception, string callsign);

    private sealed record SampleRow(string Name, StatusState State, int Count, double Value);
}
