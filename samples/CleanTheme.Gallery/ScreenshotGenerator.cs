using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using CleanTheme.Logging;
using CleanTheme.Wpf;

namespace CleanTheme.Gallery;

internal static class ScreenshotGenerator
{
    public static void Run(MainWindow window, LogFeed feed, string outputDir)
    {
        Directory.CreateDirectory(outputDir);

        window.Width = 1200;
        window.Height = 820;
        window.Show();
        PumpEvents();

        // Feed some realistic log entries for the activity logs screenshot
        feed.Post(new LogEntry(DateTime.UtcNow.AddMinutes(-5), Microsoft.Extensions.Logging.LogLevel.Information, "FleetSupervisor", "Initialized fleet manager with 3 active nodes"));
        feed.Post(new LogEntry(DateTime.UtcNow.AddMinutes(-4), Microsoft.Extensions.Logging.LogLevel.Debug, "SimulatedServer", "Listening on UDP socket 0.0.0.0:9050"));
        feed.Post(new LogEntry(DateTime.UtcNow.AddMinutes(-3), Microsoft.Extensions.Logging.LogLevel.Information, "ALPHA-1", "Waypoint ALPHA-WP1 reached; speed 420 kts"));
        feed.Post(new LogEntry(DateTime.UtcNow.AddMinutes(-2), Microsoft.Extensions.Logging.LogLevel.Warning, "BRAVO-2", "Low fuel threshold warning (12% remaining)"));
        feed.Post(new LogEntry(DateTime.UtcNow.AddMinutes(-1), Microsoft.Extensions.Logging.LogLevel.Error, "CHARLIE-3", "Engine actuator synchronization fault; code 0x80070005"));
        feed.Post(new LogEntry(DateTime.UtcNow, Microsoft.Extensions.Logging.LogLevel.Information, "TelemetrySink", "Synchronized 142 metric samples to local storage"));
        PumpEvents();

        // 1. Overview & Controls (Light Mode)
        SetTheme(CleanThemeMode.Light);
        window.NavRail.SelectedIndex = 0;
        PumpEvents();
        Capture(window, Path.Combine(outputDir, "overview-light.png"));

        // 2. Overview & Controls (Dark Mode)
        SetTheme(CleanThemeMode.Dark);
        PumpEvents();
        Capture(window, Path.Combine(outputDir, "overview-dark.png"));

        // 3. Searchable Settings (Light Mode)
        SetTheme(CleanThemeMode.Light);
        window.NavRail.SelectedIndex = 4;
        PumpEvents();
        Capture(window, Path.Combine(outputDir, "settings-light.png"));

        // 4. Searchable Settings (Dark Mode)
        SetTheme(CleanThemeMode.Dark);
        PumpEvents();
        Capture(window, Path.Combine(outputDir, "settings-dark.png"));

        // 5. Fleet Units (Light Mode)
        SetTheme(CleanThemeMode.Light);
        window.NavRail.SelectedIndex = 1;
        PumpEvents();
        Capture(window, Path.Combine(outputDir, "fleet-light.png"));

        // 6. Activity Logs (Dark Mode)
        SetTheme(CleanThemeMode.Dark);
        window.NavRail.SelectedIndex = 2;
        PumpEvents();
        Capture(window, Path.Combine(outputDir, "logs-dark.png"));
    }

    private static void SetTheme(CleanThemeMode mode)
    {
        ThemeManager.Mode = mode;
        PumpEvents();
        Thread.Sleep(50);
        PumpEvents();
    }

    private static void PumpEvents()
    {
        var frame = new DispatcherFrame();
        Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, () => frame.Continue = false);
        Dispatcher.PushFrame(frame);
    }

    private static void Capture(MainWindow window, string filePath, int width = 1200, int height = 820)
    {
        window.Measure(new Size(width, height));
        window.Arrange(new Rect(0, 0, width, height));
        window.UpdateLayout();
        PumpEvents();

        var dv = new DrawingVisual();
        using (var dc = dv.RenderOpen())
        {
            var bg = (Brush)window.FindResource("AppBackground");
            dc.DrawRectangle(bg, null, new Rect(0, 0, width, height));
            dc.DrawRectangle(new VisualBrush((Visual)window.Content), null, new Rect(0, 0, width, height));
        }

        var rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        rtb.Render(dv);

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(rtb));
        using var stream = File.Create(filePath);
        encoder.Save(stream);
    }
}
