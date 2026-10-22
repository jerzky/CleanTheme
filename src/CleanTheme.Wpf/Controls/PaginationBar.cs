using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CleanTheme.Wpf.Controls;

/// <summary>Pagination control with Previous/Next buttons, active page indicators, ellipsis jumps, and record counts.</summary>
public class PaginationBar : Control
{
    private static readonly DependencyPropertyKey PageItemsPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(PageItems), typeof(ObservableCollection<object>), typeof(PaginationBar), new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty PageItemsProperty = PageItemsPropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey SummaryTextPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(SummaryText), typeof(string), typeof(PaginationBar), new FrameworkPropertyMetadata(string.Empty));

    public static readonly DependencyProperty SummaryTextProperty = SummaryTextPropertyKey.DependencyProperty;

    public static readonly DependencyProperty CurrentPageProperty = DependencyProperty.Register(
        nameof(CurrentPage), typeof(int), typeof(PaginationBar),
        new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPagePropertyChanged, (d, v) => CoerceCurrentPage(d, v)));

    public static readonly DependencyProperty TotalPagesProperty = DependencyProperty.Register(
        nameof(TotalPages), typeof(int), typeof(PaginationBar),
        new FrameworkPropertyMetadata(1, OnPagePropertyChanged, (d, v) => CoerceTotalPages(d, v)));

    public static readonly DependencyProperty PageSizeProperty = DependencyProperty.Register(
        nameof(PageSize), typeof(int), typeof(PaginationBar),
        new FrameworkPropertyMetadata(10, OnPagePropertyChanged));

    public static readonly DependencyProperty TotalItemsProperty = DependencyProperty.Register(
        nameof(TotalItems), typeof(int?), typeof(PaginationBar),
        new FrameworkPropertyMetadata(null, OnPagePropertyChanged));

    public static readonly RoutedEvent PageChangedEvent = EventManager.RegisterRoutedEvent(
        nameof(PageChanged), RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<int>), typeof(PaginationBar));

    public static readonly RoutedCommand PreviousPageCommand = new(nameof(PreviousPageCommand), typeof(PaginationBar));
    public static readonly RoutedCommand NextPageCommand = new(nameof(NextPageCommand), typeof(PaginationBar));
    public static readonly RoutedCommand GoToPageCommand = new(nameof(GoToPageCommand), typeof(PaginationBar));

    static PaginationBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(PaginationBar), new FrameworkPropertyMetadata(typeof(PaginationBar)));

        CommandManager.RegisterClassCommandBinding(typeof(PaginationBar),
            new CommandBinding(PreviousPageCommand, (s, _) => ((PaginationBar)s).GoToPrevious(), (s, e) => e.CanExecute = ((PaginationBar)s).CurrentPage > 1));

        CommandManager.RegisterClassCommandBinding(typeof(PaginationBar),
            new CommandBinding(NextPageCommand, (s, _) => ((PaginationBar)s).GoToNext(), (s, e) => e.CanExecute = ((PaginationBar)s).CurrentPage < ((PaginationBar)s).TotalPages));

        CommandManager.RegisterClassCommandBinding(typeof(PaginationBar),
            new CommandBinding(GoToPageCommand, (s, e) =>
            {
                if (e.Parameter is int page)
                {
                    ((PaginationBar)s).CurrentPage = page;
                }
                else if (e.Parameter is string str && int.TryParse(str, out int parsed))
                {
                    ((PaginationBar)s).CurrentPage = parsed;
                }
            }));
    }

    public PaginationBar()
    {
        SetValue(PageItemsPropertyKey, new ObservableCollection<object>());
        RebuildPages();
    }

    public ObservableCollection<object> PageItems => (ObservableCollection<object>)GetValue(PageItemsProperty);

    public string SummaryText => (string)GetValue(SummaryTextProperty);

    public int CurrentPage
    {
        get => (int)GetValue(CurrentPageProperty);
        set => SetValue(CurrentPageProperty, value);
    }

    public int TotalPages
    {
        get => (int)GetValue(TotalPagesProperty);
        set => SetValue(TotalPagesProperty, value);
    }

    public int PageSize
    {
        get => (int)GetValue(PageSizeProperty);
        set => SetValue(PageSizeProperty, value);
    }

    public int? TotalItems
    {
        get => (int?)GetValue(TotalItemsProperty);
        set => SetValue(TotalItemsProperty, value);
    }

    public event RoutedPropertyChangedEventHandler<int> PageChanged
    {
        add => AddHandler(PageChangedEvent, value);
        remove => RemoveHandler(PageChangedEvent, value);
    }

    public void GoToPrevious()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
        }
    }

    public void GoToNext()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
        }
    }

    private static int CoerceTotalPages(DependencyObject d, object baseValue)
    {
        return Math.Max(1, (int)baseValue);
    }

    private static int CoerceCurrentPage(DependencyObject d, object baseValue)
    {
        var bar = (PaginationBar)d;
        int val = (int)baseValue;
        if (val < 1) return 1;
        if (val > bar.TotalPages) return bar.TotalPages;
        return val;
    }

    private static void OnPagePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PaginationBar bar)
        {
            bar.CoerceValue(CurrentPageProperty);
            bar.RebuildPages();

            if (e.Property == CurrentPageProperty && e.OldValue is int oldVal && e.NewValue is int newVal && oldVal != newVal)
            {
                bar.RaiseEvent(new RoutedPropertyChangedEventArgs<int>(oldVal, newVal, PageChangedEvent));
            }
        }
    }

    private void RebuildPages()
    {
        var list = PageItems;
        if (list == null) return;
        list.Clear();

        int total = TotalPages;
        int current = CurrentPage;

        void AddPage(int p) => list.Add(new PageToken { PageNumber = p, Text = p.ToString(System.Globalization.CultureInfo.InvariantCulture), IsActive = (p == current) });
        void AddEllipsis() => list.Add(new PageToken { PageNumber = null, Text = "…", IsActive = false });

        if (total <= 7)
        {
            for (int i = 1; i <= total; i++)
            {
                AddPage(i);
            }
        }
        else
        {
            if (current <= 4)
            {
                for (int i = 1; i <= 5; i++) AddPage(i);
                AddEllipsis();
                AddPage(total);
            }
            else if (current >= total - 3)
            {
                AddPage(1);
                AddEllipsis();
                for (int i = total - 4; i <= total; i++) AddPage(i);
            }
            else
            {
                AddPage(1);
                AddEllipsis();
                AddPage(current - 1);
                AddPage(current);
                AddPage(current + 1);
                AddEllipsis();
                AddPage(total);
            }
        }

        // Summary text
        if (TotalItems.HasValue)
        {
            int start = (current - 1) * PageSize + 1;
            int end = Math.Min(TotalItems.Value, current * PageSize);
            SetValue(SummaryTextPropertyKey, $"Showing {start}–{end} of {TotalItems.Value} results");
        }
        else
        {
            SetValue(SummaryTextPropertyKey, $"Page {current} of {total}");
        }
    }
}

/// <summary>Represents an individual page button or ellipsis token in a <see cref="PaginationBar"/>.</summary>
public sealed class PageToken
{
    public int? PageNumber { get; init; }
    public string Text { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public bool IsEllipsis => PageNumber == null;
}
