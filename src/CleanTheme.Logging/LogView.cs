using System.Windows;
using System.Windows.Controls;

namespace CleanTheme.Logging;

/// <summary>
/// Shows a <see cref="LogFeed"/>: monospace lines with the level colored, virtualized, following new lines while it is
/// scrolled to the bottom and holding still once the user scrolls up.
/// </summary>
[TemplatePart(Name = ScrollViewerPart, Type = typeof(ScrollViewer))]
public sealed class LogView : ItemsControl
{
    private const string ScrollViewerPart = "PART_ScrollViewer";

    public static readonly DependencyProperty FeedProperty = DependencyProperty.Register(
        nameof(Feed), typeof(LogFeed), typeof(LogView), new FrameworkPropertyMetadata(null, OnFeedChanged));

    public static readonly DependencyProperty AutoScrollProperty = DependencyProperty.Register(
        nameof(AutoScroll), typeof(bool), typeof(LogView), new FrameworkPropertyMetadata(true));

    private ScrollViewer? _scrollViewer;
    private bool _following = true;

    static LogView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(LogView), new FrameworkPropertyMetadata(typeof(LogView)));
    }

    /// <summary>The feed to show; its <see cref="LogFeed.Visible"/> lines become the items.</summary>
    public LogFeed? Feed
    {
        get => (LogFeed?)GetValue(FeedProperty);
        set => SetValue(FeedProperty, value);
    }

    /// <summary>Follow new lines while scrolled to the bottom. On by default.</summary>
    public bool AutoScroll
    {
        get => (bool)GetValue(AutoScrollProperty);
        set => SetValue(AutoScrollProperty, value);
    }

    public override void OnApplyTemplate()
    {
        if (_scrollViewer != null)
        {
            _scrollViewer.ScrollChanged -= OnScrollChanged;
        }
        base.OnApplyTemplate();
        _scrollViewer = GetTemplateChild(ScrollViewerPart) as ScrollViewer;
        if (_scrollViewer != null)
        {
            _scrollViewer.ScrollChanged += OnScrollChanged;
        }
    }

    private static void OnFeedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((LogView)d).ItemsSource = (e.NewValue as LogFeed)?.Visible;

    private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        var viewer = (ScrollViewer)sender;
        if (e.ExtentHeightChange == 0)
        {
            // The user (or a resize) scrolled: follow again only once back at the bottom.
            _following = viewer.VerticalOffset >= viewer.ScrollableHeight - 1;
        }
        else if (_following && AutoScroll)
        {
            viewer.ScrollToEnd();
        }
    }
}
