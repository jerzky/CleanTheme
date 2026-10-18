using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace CleanTheme.Logging;

/// <summary>
/// Shows a <see cref="LogFeed"/>: monospace lines with the level and origin, virtualized, following new lines while it
/// is scrolled to the bottom and holding still once the user scrolls up. A bar on top shows the line count, a search box
/// (the feed's text filter) and a filter popout (level, source, category); an empty state shows when no line is visible.
/// </summary>
/// <remarks>
/// Lines are selectable (click, Shift/Ctrl-click, drag, Ctrl+A). Ctrl+C or the context menu copies the selected lines,
/// <see cref="CopyAllCommand"/> every visible line, as <see cref="LogEntry.Text"/> in display order.
/// </remarks>
[TemplatePart(Name = ScrollViewerPart, Type = typeof(ScrollViewer))]
[TemplatePart(Name = FilterPopupPart, Type = typeof(Popup))]
[TemplatePart(Name = SourceBoxPart, Type = typeof(ComboBox))]
[TemplatePart(Name = CategoryBoxPart, Type = typeof(ComboBox))]
[TemplatePart(Name = ResetButtonPart, Type = typeof(ButtonBase))]
[TemplatePart(Name = EmptyResetButtonPart, Type = typeof(ButtonBase))]
public sealed class LogView : ListBox
{
    /// <summary>Copies every visible line. Bound on each LogView; also offered in its context menu.</summary>
    public static RoutedUICommand CopyAllCommand { get; } = new("Copy all lines", nameof(CopyAllCommand), typeof(LogView));

    /// <summary>The first choice of the source box, which clears <see cref="LogFeed.SourceFilter"/>.</summary>
    public const string AllSourcesLabel = "All sources";

    /// <summary>The first choice of the category box, which clears <see cref="LogFeed.CategoryFilter"/>.</summary>
    public const string AllCategoriesLabel = "All categories";

    private const string ScrollViewerPart = "PART_ScrollViewer";
    private const string FilterPopupPart = "PART_FilterPopup";
    private const string SourceBoxPart = "PART_SourceBox";
    private const string CategoryBoxPart = "PART_CategoryBox";
    private const string ResetButtonPart = "PART_ResetButton";
    private const string EmptyResetButtonPart = "PART_EmptyResetButton";

    public static readonly DependencyProperty FeedProperty = DependencyProperty.Register(
        nameof(Feed), typeof(LogFeed), typeof(LogView), new FrameworkPropertyMetadata(null, OnFeedChanged));

    public static readonly DependencyProperty AutoScrollProperty = DependencyProperty.Register(
        nameof(AutoScroll), typeof(bool), typeof(LogView), new FrameworkPropertyMetadata(true));

    public static readonly DependencyProperty ShowFilterBarProperty = DependencyProperty.Register(
        nameof(ShowFilterBar), typeof(bool), typeof(LogView), new FrameworkPropertyMetadata(true));

    public static readonly DependencyProperty ShowOriginProperty = DependencyProperty.Register(
        nameof(ShowOrigin), typeof(bool), typeof(LogView), new FrameworkPropertyMetadata(true));

    private readonly ChoiceBox _sourceChoices;
    private readonly ChoiceBox _categoryChoices;
    private ScrollViewer? _scrollViewer;
    private FrameworkElement? _filterBar;
    private ButtonBase? _resetButton;
    private ButtonBase? _emptyResetButton;
    private bool _following = true;

    static LogView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(LogView), new FrameworkPropertyMetadata(typeof(LogView)));
        SelectionModeProperty.OverrideMetadata(typeof(LogView), new FrameworkPropertyMetadata(SelectionMode.Extended));
    }

    public LogView()
    {
        CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, (_, _) => CopyToClipboard(GetSelectedText()), (_, e) => e.CanExecute = SelectedItems.Count > 0));
        CommandBindings.Add(new CommandBinding(CopyAllCommand, (_, _) => CopyToClipboard(GetAllText()), (_, e) => e.CanExecute = Items.Count > 0));
        // ListBox binds Ctrl+A to a private command of its own; this makes ApplicationCommands.SelectAll (the menu item) work too.
        CommandBindings.Add(new CommandBinding(ApplicationCommands.SelectAll, (_, _) => SelectAll(), (_, e) => e.CanExecute = Items.Count > 0));

        _sourceChoices = new ChoiceBox(
            AllSourcesLabel,
            feed => feed.Sources,
            source => source,
            feed => feed.SourceFilter,
            (feed, value) => feed.SourceFilter = value);
        _categoryChoices = new ChoiceBox(
            AllCategoriesLabel,
            feed => feed.Categories,
            category => LogEntry.ShortenCategory(category) ?? category,
            feed => feed.CategoryFilter,
            (feed, value) => feed.CategoryFilter = value);
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

    /// <summary>Show each line's <see cref="LogEntry.Origin"/> (source, else short category). On by default.</summary>
    public bool ShowOrigin
    {
        get => (bool)GetValue(ShowOriginProperty);
        set => SetValue(ShowOriginProperty, value);
    }

    public override void OnApplyTemplate()
    {
        if (_scrollViewer != null)
        {
            _scrollViewer.ScrollChanged -= OnScrollChanged;
        }
        if (_resetButton != null)
        {
            _resetButton.Click -= OnResetClick;
        }
        if (_emptyResetButton != null)
        {
            _emptyResetButton.Click -= OnResetClick;
        }

        base.OnApplyTemplate();

        _scrollViewer = GetTemplateChild(ScrollViewerPart) as ScrollViewer;
        _filterBar = GetTemplateChild("FilterBar") as FrameworkElement;
        _resetButton = GetTemplateChild(ResetButtonPart) as ButtonBase;
        _emptyResetButton = GetTemplateChild(EmptyResetButtonPart) as ButtonBase;
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
        if (_resetButton != null)
        {
            _resetButton.Click += OnResetClick;
        }
        if (_emptyResetButton != null)
        {
            _emptyResetButton.Click += OnResetClick;
        }
        _sourceChoices.Attach(GetTemplateChild(SourceBoxPart) as ComboBox, Feed);
        _categoryChoices.Attach(GetTemplateChild(CategoryBoxPart) as ComboBox, Feed);
    }

    /// <summary>The selected lines as text, in display order (not selection order), one per line.</summary>
    internal string GetSelectedText() =>
        JoinLines(SelectedItems.OfType<LogEntry>().Select(entry => (Index: Items.IndexOf(entry), Entry: entry)).OrderBy(x => x.Index).Select(x => x.Entry));

    /// <summary>Every visible line as text.</summary>
    internal string GetAllText() => JoinLines(Items.OfType<LogEntry>());

    private static string JoinLines(IEnumerable<LogEntry> entries) => string.Join(Environment.NewLine, entries.Select(entry => entry.Text));

    private static void CopyToClipboard(string text)
    {
        if (text.Length > 0)
        {
            Clipboard.SetText(text);
        }
    }

    // The stock ListBox peer reports only the lines; this one also exposes the filter bar's controls.
    protected override AutomationPeer OnCreateAutomationPeer() => new LogViewAutomationPeer(this);

    private static void OnFeedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var view = (LogView)d;
        view.ItemsSource = (e.NewValue as LogFeed)?.Visible;
        view._sourceChoices.SetFeed(e.OldValue as LogFeed, e.NewValue as LogFeed);
        view._categoryChoices.SetFeed(e.OldValue as LogFeed, e.NewValue as LogFeed);
    }

    private void OnResetClick(object sender, RoutedEventArgs e) => Feed?.ResetFilters();

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

    /// <summary>
    /// Keeps a combo box of "all" plus the values a feed has seen in step with one of the feed's filters, both ways.
    /// Feed subscriptions are weak, so a long-lived feed does not keep the view alive.
    /// </summary>
    private sealed class ChoiceBox(
        string allLabel,
        Func<LogFeed, ReadOnlyObservableCollection<string>> values,
        Func<string, string> label,
        Func<LogFeed, string?> getFilter,
        Action<LogFeed, string?> setFilter)
    {
        private ComboBox? _box;
        private LogFeed? _feed;
        private bool _syncing;

        public void Attach(ComboBox? box, LogFeed? feed)
        {
            if (_box != null)
            {
                _box.SelectionChanged -= OnSelectionChanged;
            }
            _box = box;
            _feed = feed;
            if (_box != null)
            {
                _box.SelectionChanged += OnSelectionChanged;
            }
            Sync();
        }

        public void SetFeed(LogFeed? oldFeed, LogFeed? newFeed)
        {
            if (oldFeed != null)
            {
                CollectionChangedEventManager.RemoveHandler(values(oldFeed), OnValuesChanged);
                PropertyChangedEventManager.RemoveHandler(oldFeed, OnFeedPropertyChanged, string.Empty);
            }
            if (newFeed != null)
            {
                CollectionChangedEventManager.AddHandler(values(newFeed), OnValuesChanged);
                PropertyChangedEventManager.AddHandler(newFeed, OnFeedPropertyChanged, string.Empty);
            }
            _feed = newFeed;
            Sync();
        }

        private void OnValuesChanged(object? sender, NotifyCollectionChangedEventArgs e) => Sync();

        private void OnFeedPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Filters change rarely; TotalCount changes with every batch and is ignored here.
            if (e.PropertyName != nameof(LogFeed.TotalCount) && e.PropertyName != nameof(LogFeed.IsFiltered))
            {
                Sync();
            }
        }

        /// <summary>Rebuilds the choices (Tag = filter value, null for "all") and selects the feed's current filter.</summary>
        private void Sync()
        {
            if (_box is null)
            {
                return;
            }
            _syncing = true;
            try
            {
                var choices = new List<ComboBoxItem> { Choice(allLabel, null) };
                if (_feed != null)
                {
                    choices.AddRange(values(_feed).Select(value => Choice(label(value), value)));
                }
                _box.ItemsSource = choices;
                var filter = _feed is null ? null : getFilter(_feed);
                _box.SelectedIndex = Math.Max(0, choices.FindIndex(choice => Equals(choice.Tag, filter)));
            }
            finally
            {
                _syncing = false;
            }
        }

        private ComboBoxItem Choice(string text, string? value) => new()
        {
            Content = text,
            Tag = value,
            ToolTip = value != null && value != text ? value : null,
            Style = _box?.ItemContainerStyle,
        };

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_syncing && _feed != null && _box?.SelectedItem is ComboBoxItem choice)
            {
                setFilter(_feed, (string?)choice.Tag);
            }
        }
    }

    /// <summary>A list (so lines keep their selection semantics) whose first children are the filter bar's controls.</summary>
    private sealed class LogViewAutomationPeer(LogView owner) : ListBoxAutomationPeer(owner)
    {
        protected override string GetClassNameCore() => nameof(LogView);

        protected override List<AutomationPeer> GetChildrenCore()
        {
            var children = base.GetChildrenCore() ?? [];
            if (owner._filterBar is { IsVisible: true } bar)
            {
                children.InsertRange(0, PeersBelow(bar));
            }
            return children;
        }

        // Elements without a peer of their own (borders, panels) are walked through to the controls inside.
        private static IEnumerable<AutomationPeer> PeersBelow(DependencyObject parent)
        {
            for (var i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is UIElement element && UIElementAutomationPeer.CreatePeerForElement(element) is { } peer)
                {
                    yield return peer;
                }
                else
                {
                    foreach (var nested in PeersBelow(child))
                    {
                        yield return nested;
                    }
                }
            }
        }
    }
}
