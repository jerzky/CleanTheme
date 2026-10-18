using System.Windows;

namespace CleanTheme.Gallery;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Rows.ItemsSource = new[]
        {
            new SampleRow("ALPHA-1", "Running", 12, 412.5),
            new SampleRow("BRAVO-2", "Stopped", 3, 0),
            new SampleRow("CHARLIE-3", "Running", 27, 1288.25),
        };
        Log.ItemsSource = new[]
        {
            "12:00:01.204 INF Host started",
            "12:00:01.388 INF Listening on 127.0.0.1:9050",
            "12:00:02.015 WRN Slow tick: 18.4 ms",
        };
    }

    private sealed record SampleRow(string Name, string State, int Count, double Value);
}
