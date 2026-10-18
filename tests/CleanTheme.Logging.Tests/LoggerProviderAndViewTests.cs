using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using CleanTheme.Wpf.Tests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CleanTheme.Logging.Tests;

public sealed partial class LoggerProviderAndViewTests
{
    private static readonly Func<ILogger, string, IDisposable?> SourceScope = LoggerMessage.DefineScope<string>("{Source}");

    [LoggerMessage(Level = LogLevel.Warning, Message = "Tick took {Ms} ms")]
    private static partial void LogSlowTick(ILogger logger, Exception exception, int ms);

    [LoggerMessage(Level = LogLevel.Information, Message = "{Text}")]
    private static partial void LogText(ILogger logger, string text);

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

        Assert.Equal([LogView.AllSourcesLabel, "A"], Labels(sourceBox));
        Assert.Equal(0, sourceBox.SelectedIndex);

        feed.Add(new LogEntry(DateTime.UtcNow, LogLevel.Information, "B", "b"));
        Assert.Equal([LogView.AllSourcesLabel, "A", "B"], Labels(sourceBox));

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

    [Fact]
    public void A_Source_scope_sets_the_line_source_and_the_innermost_wins() => Sta.Run(() =>
    {
        var services = new ServiceCollection().AddLogging(logging => logging.AddLogFeed());
        using var provider = services.BuildServiceProvider();
        var feed = provider.GetRequiredService<LogFeed>();
        var logger = provider.GetRequiredService<ILogger<LoggerProviderAndViewTests>>();

        LogText(logger, "outside");
        using (SourceScope(logger, "UAV-1"))
        {
            LogText(logger, "template scope");
            using (logger.BeginScope(new Dictionary<string, object> { ["Source"] = "AWACS-1", ["Other"] = 1 }))
            {
                LogText(logger, "dictionary scope");
            }
            using (logger.BeginScope(new Dictionary<string, object> { ["Unrelated"] = 2 }))
            {
                LogText(logger, "unrelated scope");
            }
        }
        Sta.DoEvents();

        Assert.Equal(
            [("outside", null), ("template scope", "UAV-1"), ("dictionary scope", "AWACS-1"), ("unrelated scope", "UAV-1")],
            feed.Visible.Select(e => (e.Message, e.Source)));
        Assert.Equal(["UAV-1", "AWACS-1"], feed.Sources);
    });

    [Fact]
    public void Scopes_are_shared_with_the_factory_scope_provider() => Sta.Run(() =>
    {
        var services = new ServiceCollection().AddLogging(logging => logging.AddLogFeed());
        using var provider = services.BuildServiceProvider();
        var feed = provider.GetRequiredService<LogFeed>();
        var factory = provider.GetRequiredService<ILoggerFactory>();
        var outer = factory.CreateLogger("Outer");
        var inner = factory.CreateLogger("Inner");

        // A scope opened on one category's logger applies to another's, as with the console provider.
        using (SourceScope(outer, "UAV-2"))
        {
            LogText(inner, "hello");
        }
        Sta.DoEvents();

        Assert.Equal("UAV-2", Assert.Single(feed.Visible).Source);
    });

    [Fact]
    public void Filter_popout_category_box_shows_short_names_and_filters_by_full_category() => Sta.Run(() =>
    {
        var feed = LogFeed.CreateForCurrentThread();
        feed.Add(new LogEntry(DateTime.UtcNow, LogLevel.Information, null, "a") { Category = "App.Services.NodeSupervisor" });
        feed.Add(new LogEntry(DateTime.UtcNow, LogLevel.Information, null, "b") { Category = "App.Server" });
        var view = new LogView { Feed = feed };
        Sta.Layout(view);
        var categoryBox = (ComboBox)view.Template.FindName("PART_CategoryBox", view);

        Assert.Equal([LogView.AllCategoriesLabel, "NodeSupervisor", "Server"], Labels(categoryBox));
        Assert.Equal("App.Services.NodeSupervisor", ((ComboBoxItem)categoryBox.Items[1]).ToolTip);

        categoryBox.SelectedIndex = 1;
        Assert.Equal("App.Services.NodeSupervisor", feed.CategoryFilter);
        Assert.Equal(["a"], feed.Visible.Select(e => e.Message));

        feed.ResetFilters();
        Assert.Equal(0, categoryBox.SelectedIndex);
    });

    [Fact]
    public void LogView_selects_lines_and_copies_them_in_display_order() => Sta.Run(() =>
    {
        var feed = LogFeed.CreateForCurrentThread();
        var t = new DateTime(2026, 10, 18, 12, 0, 0, DateTimeKind.Utc);
        feed.Add(new LogEntry(t, LogLevel.Information, "UAV-1", "first"));
        feed.Add(new LogEntry(t, LogLevel.Warning, null, "second") { Category = "App.Server" });
        feed.Add(new LogEntry(t, LogLevel.Error, null, "third"));
        var view = new LogView { Feed = feed };
        Sta.Layout(view);

        Assert.Equal(SelectionMode.Extended, view.SelectionMode);
        Assert.False(ApplicationCommands.Copy.CanExecute(null, view));

        // Selected last line first: the copy still follows the display order.
        view.SelectedItems.Add(feed.Visible[2]);
        view.SelectedItems.Add(feed.Visible[0]);

        Assert.True(ApplicationCommands.Copy.CanExecute(null, view));
        Assert.Equal(
            $"12:00:00.000  INF  UAV-1  first{Environment.NewLine}12:00:00.000  ERR  third",
            view.GetSelectedText());
        Assert.Equal(3, view.GetAllText().Split(Environment.NewLine).Length);

        ApplicationCommands.SelectAll.Execute(null, view);
        Assert.Equal(3, view.SelectedItems.Count);
    });

    [Fact]
    public void LogView_context_menu_offers_copy_copy_all_and_select_all() => Sta.Run(() =>
    {
        var view = new LogView { Feed = LogFeed.CreateForCurrentThread() };
        Sta.Layout(view);

        var commands = view.ContextMenu.Items.Cast<MenuItem>().Select(item => item.Command).ToArray();

        Assert.Equal([ApplicationCommands.Copy, LogView.CopyAllCommand, ApplicationCommands.SelectAll], commands);
        // Each view gets its own menu: a ContextMenu has a single owner.
        var other = new LogView();
        Sta.Layout(other);
        Assert.NotSame(view.ContextMenu, other.ContextMenu);
    });

    [Fact]
    public void LogView_search_box_edits_the_text_filter_both_ways() => Sta.Run(() =>
    {
        var feed = LogFeed.CreateForCurrentThread();
        feed.Add(new LogEntry(DateTime.UtcNow, LogLevel.Information, null, "Tick took 9 ms"));
        feed.Add(new LogEntry(DateTime.UtcNow, LogLevel.Information, null, "Client connected"));
        var view = new LogView { Feed = feed };
        Sta.Layout(view);
        var search = (CleanTheme.Wpf.Controls.SearchBox)view.Template.FindName("PART_SearchBox", view);

        search.Text = "tick";
        Sta.DoEventsFor(TimeSpan.FromMilliseconds(350)); // The binding waits 200 ms after typing.
        Assert.Equal("tick", feed.TextFilter);
        Assert.Single(feed.Visible);

        feed.ResetFilters();
        Assert.Equal("", search.Text);
    });

    [Fact]
    public void LogView_empty_state_says_why_nothing_shows_and_resets_filters() => Sta.Run(() =>
    {
        var feed = LogFeed.CreateForCurrentThread();
        var view = new LogView { Feed = feed };
        Sta.Layout(view);
        var empty = (CleanTheme.Wpf.Controls.EmptyState)view.Template.FindName("Empty", view);
        var reset = (ButtonBase)view.Template.FindName("PART_EmptyResetButton", view);

        Assert.Equal(System.Windows.Visibility.Visible, empty.Visibility);
        Assert.Equal("No log lines yet", empty.Title);
        Assert.Equal(System.Windows.Visibility.Collapsed, reset.Visibility);

        feed.Add(new LogEntry(DateTime.UtcNow, LogLevel.Information, null, "hello"));
        Sta.Layout(view);
        Assert.Equal(System.Windows.Visibility.Collapsed, empty.Visibility);

        feed.TextFilter = "nothing like this";
        Sta.Layout(view);
        Assert.Equal(System.Windows.Visibility.Visible, empty.Visibility);
        Assert.Equal("No lines match the filters", empty.Title);
        Assert.Equal(System.Windows.Visibility.Visible, reset.Visibility);

        reset.RaiseEvent(new System.Windows.RoutedEventArgs(ButtonBase.ClickEvent));
        Assert.False(feed.IsFiltered);
    });

    private static IEnumerable<string> Labels(ComboBox box) => box.Items.Cast<ComboBoxItem>().Select(item => (string)item.Content);
}
