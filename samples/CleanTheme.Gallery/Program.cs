using CleanTheme.Gallery.Services;
using CleanTheme.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CleanTheme.Gallery;

/// <summary>Entry point and composition root: a Generic Host whose services log through ILogger into the gallery's LogView.</summary>
public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        // The in-app log. Built here because the feed belongs to this (STA, UI) thread's dispatcher.
        // Debug and up, so the gallery shows every level; a real app would usually start at Information.
        builder.Logging.AddLogFeed();
        builder.Logging.AddFilter<LogFeedLoggerProvider>(null, LogLevel.Debug);

        builder.Services.AddHostedService<SimulatedServer>();
        builder.Services.AddHostedService<SimulatedWorkspaceActivity>();
        builder.Services.AddSingleton<MainWindow>();

        using var host = builder.Build();
        var app = new App(host);
        app.InitializeComponent();

        if (args.Length > 0 && args[0] == "--screenshots")
        {
            var outputDir = args.Length > 1 ? args[1] : "docs/images";
            var window = host.Services.GetRequiredService<MainWindow>();
            var feed = host.Services.GetRequiredService<LogFeed>();
            ScreenshotGenerator.Run(window, feed, outputDir);
            window.Close();
            return;
        }

        app.Run();
    }
}
