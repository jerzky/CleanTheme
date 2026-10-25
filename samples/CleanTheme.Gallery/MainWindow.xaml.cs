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
            new SampleRow("Core Platform API", StatusState.Positive, 24, 1420.0),
            new SampleRow("Billing Pipeline", StatusState.Warning, 3, 310.5),
            new SampleRow("Analytics Worker", StatusState.Negative, 7, 890.0),
            new SampleRow("Frontend Portal", StatusState.Positive, 18, 2150.0),
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
            throw new InvalidOperationException("Connection timeout while contacting database");
        }
        catch (InvalidOperationException ex)
        {
            LogServiceFailed(_logger, ex, "analytics-worker");
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
            StatusState.Positive => "Workspace saved as workspace-prod.json",
            StatusState.Warning => "Billing pipeline backlog elevated",
            StatusState.Negative => "Analytics worker connection timeout",
            _ => "New workspace created",
        };
        Toasts.Show(message, state);
    }

    private void OnMessage(object sender, RoutedEventArgs e) =>
        CleanMessageBox.Show(this, "The workspace was saved to the Projects folder.", "Workspace", "Saved");

    private void OnConfirmDelete(object sender, RoutedEventArgs e)
    {
        var confirmed = CleanMessageBox.Confirm(this, "This removes the workspace configuration from disk. Cloud services are not affected.",
            "Delete workspace", "Delete", heading: "Delete workspace-prod.json?", isDanger: true);
        Toasts.Show(confirmed ? "Workspace deleted" : "Kept the workspace", confirmed ? StatusState.Positive : StatusState.Neutral);
    }

    private void OnPrompt(object sender, RoutedEventArgs e)
    {
        var name = CleanMessageBox.Prompt(this, "Save workspace", "Name for the new workspace configuration", "New workspace", "Save");
        if (name != null)
        {
            Toasts.Show($"Saved \"{name}\"", StatusState.Positive);
        }
    }

    private void OnNewScenario(object sender, RoutedEventArgs e) => Toasts.Show("New workspace (sample template)");

    private void OnAbout(object sender, RoutedEventArgs e) =>
        CleanMessageBox.Show(this, "Clean, modern Windows desktop presentation theme for WPF.", "About", "CleanTheme Gallery");

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

    [LoggerMessage(Level = LogLevel.Error, Message = "Service {ServiceName} failed")]
    private static partial void LogServiceFailed(ILogger logger, Exception exception, string serviceName);

    private sealed record SampleRow(string Name, StatusState State, int Count, double Value);
}
