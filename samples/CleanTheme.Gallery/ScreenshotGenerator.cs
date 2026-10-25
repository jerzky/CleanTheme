using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using CleanTheme.Logging;
using CleanTheme.Wpf;
using CleanTheme.Wpf.Controls;

namespace CleanTheme.Gallery;

internal static class ScreenshotGenerator
{
    public static void Run(MainWindow window, LogFeed feed, string outputDir)
    {
        Directory.CreateDirectory(outputDir);

        window.Width = 1200;
        window.Height = 900;
        window.Show();
        PumpEvents();

        // Feed some realistic log entries for the activity logs screenshot
        feed.Post(new LogEntry(DateTime.UtcNow.AddMinutes(-5), Microsoft.Extensions.Logging.LogLevel.Information, "WorkspaceSupervisor", "Initialized workspace coordinator with 4 active pipelines"));
        feed.Post(new LogEntry(DateTime.UtcNow.AddMinutes(-4), Microsoft.Extensions.Logging.LogLevel.Debug, "SimulatedServer", "Listening on HTTPS socket 0.0.0.0:8080"));
        feed.Post(new LogEntry(DateTime.UtcNow.AddMinutes(-3), Microsoft.Extensions.Logging.LogLevel.Information, "CoreAPI", "Batch synchronization completed; 1,420 events processed"));
        feed.Post(new LogEntry(DateTime.UtcNow.AddMinutes(-2), Microsoft.Extensions.Logging.LogLevel.Warning, "BillingPipeline", "Elevated queue backlog warning (84% buffer capacity)"));
        feed.Post(new LogEntry(DateTime.UtcNow.AddMinutes(-1), Microsoft.Extensions.Logging.LogLevel.Error, "AnalyticsWorker", "Connection timeout to analytical database; code 0x80070005"));
        feed.Post(new LogEntry(DateTime.UtcNow, Microsoft.Extensions.Logging.LogLevel.Information, "TelemetrySink", "Synchronized 184 metric samples to local storage"));
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

        // 5. Workspaces (Light Mode)
        SetTheme(CleanThemeMode.Light);
        window.NavRail.SelectedIndex = 1;
        PumpEvents();
        Capture(window, Path.Combine(outputDir, "fleet-light.png"));
        Capture(window, Path.Combine(outputDir, "workspaces-light.png"));

        // 6. Activity Logs (Dark Mode)
        SetTheme(CleanThemeMode.Dark);
        window.NavRail.SelectedIndex = 2;
        PumpEvents();
        Capture(window, Path.Combine(outputDir, "logs-dark.png"));

        // 7. Animated GIFs
        GenerateHamburgerMenuGif(window, Path.Combine(outputDir, "hamburger-menu-light.gif"), CleanThemeMode.Light);
        GenerateHamburgerMenuGif(window, Path.Combine(outputDir, "hamburger-menu-dark.gif"), CleanThemeMode.Dark);
        GenerateSettingsSearchGif(window, Path.Combine(outputDir, "settings-search.gif"));
    }

    public static void GenerateHamburgerMenuGif(MainWindow window, string outputGifPath, CleanThemeMode mode)
    {
        SetTheme(mode);
        window.NavRail.SelectedIndex = 0; // Overview & Controls
        window.NavRail.IsPaneOpen = true;
        PumpEvents();

        string tempDir = Path.Combine(Path.GetTempPath(), "cleantheme_gif_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var pane = (Border)window.NavRail.Template.FindName("PART_Pane", window.NavRail);
            double openWidth = window.NavRail.OpenPaneWidth;
            double compactWidth = window.NavRail.CompactPaneWidth;

            int frameNumber = 0;
            void SaveFrame()
            {
                string framePath = Path.Combine(tempDir, $"frame_{frameNumber:D4}.png");
                Capture(window, framePath);
                frameNumber++;
            }

            // Phase 1: Hold open (15 frames = 0.5s at 30fps)
            window.NavRail.IsPaneOpen = true;
            pane.BeginAnimation(FrameworkElement.WidthProperty, null);
            pane.Width = openWidth;
            PumpEvents();
            for (int i = 0; i < 15; i++) SaveFrame();

            // Phase 2: Animate closing (12 frames = 0.4s)
            window.NavRail.IsPaneOpen = false;
            int closeSteps = 12;
            for (int i = 1; i <= closeSteps; i++)
            {
                double t = (double)i / closeSteps;
                double progress = 1.0 - Math.Pow(1.0 - t, 3); // Cubic ease out
                double w = openWidth - (openWidth - compactWidth) * progress;
                pane.BeginAnimation(FrameworkElement.WidthProperty, null);
                pane.Width = w;
                PumpEvents();
                SaveFrame();
            }

            // Phase 3: Hold compact (18 frames = 0.6s)
            window.NavRail.IsPaneOpen = false;
            pane.BeginAnimation(FrameworkElement.WidthProperty, null);
            pane.Width = compactWidth;
            PumpEvents();
            for (int i = 0; i < 18; i++) SaveFrame();

            // Phase 4: Animate opening (12 frames = 0.4s)
            window.NavRail.IsPaneOpen = true;
            int openSteps = 12;
            for (int i = 1; i <= openSteps; i++)
            {
                double t = (double)i / openSteps;
                double progress = 1.0 - Math.Pow(1.0 - t, 3);
                double w = compactWidth + (openWidth - compactWidth) * progress;
                pane.BeginAnimation(FrameworkElement.WidthProperty, null);
                pane.Width = w;
                PumpEvents();
                SaveFrame();
            }

            // Phase 5: Hold open (15 frames = 0.5s)
            pane.BeginAnimation(FrameworkElement.WidthProperty, null);
            pane.Width = openWidth;
            PumpEvents();
            for (int i = 0; i < 15; i++) SaveFrame();

            // Convert to high-quality optimized GIF using ffmpeg
            string ffmpegArgs = $"-y -framerate 30 -i \"{Path.Combine(tempDir, "frame_%04d.png")}\" -vf \"scale=1000:-1:flags=lanczos,split[s0][s1];[s0]palettegen=max_colors=256:stats_mode=diff[p];[s1][p]paletteuse=dither=bayer:bayer_scale=1\" -loop 0 \"{outputGifPath}\"";
            var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = ffmpegArgs,
                UseShellExecute = false,
                CreateNoWindow = true
            });
            process?.WaitForExit();
        }
        finally
        {
            try { Directory.Delete(tempDir, true); } catch { }
        }
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

    private static void Capture(MainWindow window, string filePath, int width = 1200, int height = 900)
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

    public static void GenerateSettingsSearchGif(MainWindow window, string outputGifPath)
    {
        SetTheme(CleanThemeMode.Light);
        window.NavRail.SelectedIndex = 4; // Settings
        window.NavRail.IsPaneOpen = true;
        PumpEvents();

        string tempDir = Path.Combine(Path.GetTempPath(), "cleantheme_settings_gif_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var settingsView = FindVisualChild<SearchableSettingsView>(window);
            if (settingsView is null) return;

            int frameNumber = 0;
            void SaveFrame()
            {
                string framePath = Path.Combine(tempDir, $"frame_{frameNumber:D4}.png");
                Capture(window, framePath);
                frameNumber++;
            }

            // Phase 1: Hold initial settings view (12 frames = 0.6s at 20fps)
            settingsView.SearchText = "";
            PumpEvents();
            for (int i = 0; i < 12; i++) SaveFrame();

            // Phase 2: Type "Password" char by char
            string term1 = "Password";
            for (int i = 1; i <= term1.Length; i++)
            {
                settingsView.SearchText = term1.Substring(0, i);
                PumpEvents();
                SaveFrame();
                SaveFrame();
            }
            // Hold Password results (14 frames = 0.7s)
            for (int i = 0; i < 14; i++) SaveFrame();

            // Phase 3: Backspace
            for (int i = term1.Length - 1; i >= 0; i--)
            {
                settingsView.SearchText = term1.Substring(0, i);
                PumpEvents();
                SaveFrame();
            }

            // Phase 4: Type "Country"
            string term2 = "Country";
            for (int i = 1; i <= term2.Length; i++)
            {
                settingsView.SearchText = term2.Substring(0, i);
                PumpEvents();
                SaveFrame();
                SaveFrame();
            }
            // Hold Country results (14 frames = 0.7s)
            for (int i = 0; i < 14; i++) SaveFrame();

            // Phase 5: Clear and hold (12 frames)
            settingsView.SearchText = "";
            PumpEvents();
            for (int i = 0; i < 12; i++) SaveFrame();

            // Convert to GIF using ffmpeg
            string ffmpegArgs = $"-y -framerate 20 -i \"{Path.Combine(tempDir, "frame_%04d.png")}\" -vf \"scale=960:-1:flags=lanczos,split[s0][s1];[s0]palettegen=max_colors=128:stats_mode=diff[p];[s1][p]paletteuse=dither=bayer:bayer_scale=3\" -loop 0 \"{outputGifPath}\"";
            var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = ffmpegArgs,
                UseShellExecute = false,
                CreateNoWindow = true
            });
            process?.WaitForExit();
        }
        finally
        {
            try { Directory.Delete(tempDir, true); } catch { }
        }
    }

    private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T typed) return typed;
            var descendant = FindVisualChild<T>(child);
            if (descendant is not null) return descendant;
        }
        return null;
    }
}
