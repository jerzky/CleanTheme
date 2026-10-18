using System.Windows.Controls;
using CleanTheme.Wpf.Tests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CleanTheme.Logging.Tests;

public sealed partial class LoggerProviderAndViewTests
{
    [LoggerMessage(Level = LogLevel.Warning, Message = "Tick took {Ms} ms")]
    private static partial void LogSlowTick(ILogger logger, Exception exception, int ms);

    [Fact]
    public void ILogger_output_lands_in_the_registered_feed() => Sta.Run(() =>
    {
        var services = new ServiceCollection().AddLogging(logging => logging.AddLogFeed());
        using var provider = services.BuildServiceProvider();
        var feed = provider.GetRequiredService<LogFeed>();
        var logger = provider.GetRequiredService<ILogger<LoggerProviderAndViewTests>>();

        // A background thread, as in a hosted service.
        var thread = new Thread(() => LogSlowTick(logger, new InvalidOperationException("boom"), 18));
        thread.Start();
        thread.Join();
        Sta.DoEvents();

        var entry = Assert.Single(feed.Visible);
        Assert.Equal(LogLevel.Warning, entry.Level);
        Assert.Null(entry.Source);
        Assert.Equal(typeof(LoggerProviderAndViewTests).FullName, entry.Category);
        Assert.StartsWith("Tick took 18 ms", entry.Message, StringComparison.Ordinal);
        Assert.Contains("boom", entry.Message, StringComparison.Ordinal);
    });

    [Fact]
    public void AddLogFeed_twice_registers_one_feed_and_one_provider() => Sta.Run(() =>
    {
        var services = new ServiceCollection().AddLogging(logging => logging.AddLogFeed().AddLogFeed());

        Assert.Single(services, d => d.ServiceType == typeof(LogFeed));
        Assert.Single(services, d => d.ImplementationType == typeof(LogFeedLoggerProvider));
    });

    [Fact]
    public void LogView_shows_the_feed_lines_in_its_scroll_viewer() => Sta.Run(() =>
    {
        var feed = LogFeed.CreateForCurrentThread();
        feed.Add(new LogEntry(DateTime.UtcNow, LogLevel.Information, null, "hello"));
        var view = new LogView { Feed = feed };
        Sta.Layout(view);

        Assert.Same(feed.Visible, view.ItemsSource);
        Assert.IsType<ScrollViewer>(view.Template.FindName("PART_ScrollViewer", view));
        Assert.NotNull(view.ItemContainerGenerator.ContainerFromIndex(0));

        view.Feed = null;
        Assert.Null(view.ItemsSource);
    });
}
