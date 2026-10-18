using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace CleanTheme.Logging;

/// <summary>
/// Shows a <see cref="LogFeed"/>: monospace lines with the level colored, virtualized, following new lines while it is
/// scrolled to the bottom and holding still once the user scrolls up. A bar on top shows the line count and a filter
/// popout (level, source, text) that edits the feed's filters.
/// </summary>
[TemplatePart(Name = ScrollViewerPart, Type = typeof(ScrollViewer))]
[TemplatePart(Name = FilterPopupPart, Type = typeof(Popup))]
[TemplatePart(Name = SourceBoxPart, Type = typeof(ComboBox))]
[TemplatePart(Name = ResetButtonPart, Type = typeof(ButtonBase))]
public sealed class LogView : ItemsControl
{
    /// <summary>The first choice of the source box, which clears <see cref="LogFeed.SourceFilter"/>.</summary>
    public const string AllSourcesLabel = "All sources";

    private const string ScrollViewerPart = "PART_ScrollViewer";
    private const string FilterPopupPart = "PART_FilterPopup";
    private const string SourceBoxPart = "PART_SourceBox";
    private const string ResetButtonPart = "PART_ResetButton";

    public static readonly DependencyProperty FeedProperty = DependencyProperty.Register(
        nameof(Feed), typeof(LogFeed), typeof(LogView), new FrameworkPropertyMetadata(null, OnFeedChanged));

    public static readonly DependencyProperty AutoScrollProperty = DependencyProperty.Register(
        nameof(AutoScroll), typeof(bool), typeof(LogView), new FrameworkPropertyMetadata(true));

    public static readonly DependencyProperty ShowFilterBarProperty = DependencyProperty.Register(
        nameof(ShowFilterBar), typeof(bool), typeof(LogView), new FrameworkPropertyMetadata(true));

    private ScrollViewer? _scrollViewer;
    private ComboBox? _sourceBox;
    private ButtonBase? _resetButton;
    private bool _following = true;
    private bool _syncingSources;

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

    /// <summary>Show the bar with the line count and the filter popout. On by default.</summary>
    public bool ShowFilterBar
    {
        get => (bool)GetValue(ShowFilterBarProperty);
        set => SetValue(ShowFilterBarProperty, value);
    }

    public override void OnApplyTemplate()
    {
        if (_scrollViewer != null)
        {
            _scrollViewer.ScrollChanged -= OnScrollChanged;
        }
        if (_sourceBox != null)
        {
            _sourceBox.SelectionChanged -= OnSourceBoxSelectionChanged;
        }
        if (_resetButton != null)
        {
            _resetButton.Click -= OnResetClick;
        }

        base.OnApplyTemplate();

        _scrollViewer = GetTemplateChild(ScrollViewerPart) as ScrollViewer;
        _sourceBox = GetTemplateChild(SourceBoxPart) as ComboBox;
        _resetButton = GetTemplateChild(ResetButtonPart) as ButtonBase;
        if (GetTemplateChild(FilterPopupPart) is Popup popup)
        {
            // Right-align the popout under the filter button.
            popup.Placement = PlacementMode.Custom;
            popup.CustomPopupPlacementCallback = (popupSize, targetSize, _) =>
                [new CustomPopupPlacement(new Point(targetSize.Width - popupSize.Width, targetSize.Height + 4), PopupPrimaryAxis.Horizontal)];
        }

        if (_scrollViewer != null)
        {
            _scrollViewer.ScrollChanged += OnScrollChanged;
        }
        if (_sourceBox != null)
        {
            _sourceBox.SelectionChanged += OnSourceBoxSelectionChanged;
        }
        if (_resetButton != null)
        {
            _resetButton.Click += OnResetClick;
        }
        SyncSourceBox();
    }

    private static void OnFeedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var view = (LogView)d;
        // Weak subscriptions: a long-lived feed must not keep a closed view alive.
        if (e.OldValue is LogFeed oldFeed)
        {
            CollectionChangedEventManager.RemoveHandler(oldFeed.Sources, view.OnSourcesChanged);
            PropertyChangedEventManager.RemoveHandler(oldFeed, view.OnFeedPropertyChanged, nameof(LogFeed.SourceFilter));
        }
        if (e.NewValue is LogFeed newFeed)
        {
            CollectionChangedEventManager.AddHandler(newFeed.Sources, view.OnSourcesChanged);
            PropertyChangedEventManager.AddHandler(newFeed, view.OnFeedPropertyChanged, nameof(LogFeed.SourceFilter));
        }
        view.ItemsSource = (e.NewValue as LogFeed)?.Visible;
        view.SyncSourceBox();
    }

    private void OnSourcesChanged(object? sender, NotifyCollectionChangedEventArgs e) => SyncSourceBox();

    private void OnFeedPropertyChanged(object? sender, PropertyChangedEventArgs e) => SyncSourceBox();

    /// <summary>Rebuilds the source choices (all sources first) and selects the feed's current filter.</summary>
    private void SyncSourceBox()
    {
        if (_sourceBox is null)
        {
            return;
        }
        _syncingSources = true;
        try
        {
            var choices = new List<string> { AllSourcesLabel };
            if (Feed != null)
            {
                choices.AddRange(Feed.Sources);
            }
            _sourceBox.ItemsSource = choices;
            var filter = Feed?.SourceFilter;
            _sourceBox.SelectedIndex = filter is null ? 0 : choices.IndexOf(filter, 1) is var i and > 0 ? i : 0;
        }
        finally
        {
            _syncingSources = false;
        }
    }

    private void OnSourceBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_syncingSources || Feed is null || _sourceBox is null || _sourceBox.SelectedIndex < 0)
        {
            return;
        }
        Feed.SourceFilter = _sourceBox.SelectedIndex == 0 ? null : (string)_sourceBox.SelectedItem;
    }

    private void OnResetClick(object sender, RoutedEventArgs e) => Feed?.ResetFilters();

    // The stock ItemsControl peer reports only items, hiding the filter bar from screen readers and UI tests.
    protected override AutomationPeer OnCreateAutomationPeer() => new LogViewAutomationPeer(this);

    private sealed class LogViewAutomationPeer(LogView owner) : FrameworkElementAutomationPeer(owner)
    {
        protected override string GetClassNameCore() => nameof(LogView);

        protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Group;
    }

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
