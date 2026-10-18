using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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

    [Fact]
    public void Filter_popout_source_box_lists_sources_and_edits_the_feed_filter() => Sta.Run(() =>
    {
        var feed = LogFeed.CreateForCurrentThread();
        feed.Add(new LogEntry(DateTime.UtcNow, LogLevel.Information, "A", "a"));
        var view = new LogView { Feed = feed };
        Sta.Layout(view);
        var sourceBox = (ComboBox)view.Template.FindName("PART_SourceBox", view);

        Assert.Equal([LogView.AllSourcesLabel, "A"], sourceBox.Items.Cast<string>());
        Assert.Equal(0, sourceBox.SelectedIndex);

        feed.Add(new LogEntry(DateTime.UtcNow, LogLevel.Information, "B", "b"));
        Assert.Equal([LogView.AllSourcesLabel, "A", "B"], sourceBox.Items.Cast<string>());

        sourceBox.SelectedIndex = 2;
        Assert.Equal("B", feed.SourceFilter);

        feed.SourceFilter = "A";
        Assert.Equal(1, sourceBox.SelectedIndex);

        feed.SourceFilter = null;
        Assert.Equal(0, sourceBox.SelectedIndex);
    });

    [Fact]
    public void Filter_popout_level_choice_and_reset_button_drive_the_feed() => Sta.Run(() =>
    {
        var feed = LogFeed.CreateForCurrentThread();
        var view = new LogView { Feed = feed };
        Sta.Layout(view);
        var levels = (Selector)view.Template.FindName("LevelChoices", view);
        var reset = (ButtonBase)view.Template.FindName("PART_ResetButton", view);

        levels.SelectedIndex = 2;
        Assert.Equal(LogLevel.Warning, feed.MinimumLevel);

        feed.MinimumLevel = LogLevel.Error;
        Assert.Equal(LogLevel.Error, levels.SelectedValue);

        feed.TextFilter = "x";
        reset.RaiseEvent(new System.Windows.RoutedEventArgs(ButtonBase.ClickEvent));

        Assert.False(feed.IsFiltered);
        Assert.Equal(LogLevel.Trace, levels.SelectedValue);
    });

    [Fact]
    public void Filter_bar_can_be_hidden() => Sta.Run(() =>
    {
        var view = new LogView { Feed = LogFeed.CreateForCurrentThread(), ShowFilterBar = false };
        Sta.Layout(view);

        var bar = (System.Windows.FrameworkElement)view.Template.FindName("FilterBar", view);
        Assert.Equal(System.Windows.Visibility.Collapsed, bar.Visibility);
    });
}
