using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace CleanTheme.Wpf.Controls;

/// <summary>
/// A collapsible group/section inside a <see cref="NavTree"/>.
/// Displays a header with expand/collapse chevron, optional count badge, and optional right-aligned action button.
/// </summary>
public class NavTreeGroup : HeaderedItemsControl
{
    public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(
        nameof(IsExpanded), typeof(bool), typeof(NavTreeGroup),
        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty BadgeProperty = DependencyProperty.Register(
        nameof(Badge), typeof(object), typeof(NavTreeGroup),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty ActionProperty = DependencyProperty.Register(
        nameof(Action), typeof(object), typeof(NavTreeGroup),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty ActionTemplateProperty = DependencyProperty.Register(
        nameof(ActionTemplate), typeof(DataTemplate), typeof(NavTreeGroup),
        new FrameworkPropertyMetadata(null));

    static NavTreeGroup()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(NavTreeGroup), new FrameworkPropertyMetadata(typeof(NavTreeGroup)));
    }

    /// <summary>Gets or sets whether the group is expanded to show child items.</summary>
    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    /// <summary>Optional count or badge label (e.g. 3, "New") displayed next to the group header.</summary>
    public object? Badge
    {
        get => GetValue(BadgeProperty);
        set => SetValue(BadgeProperty, value);
    }

    /// <summary>Optional right-aligned action element (e.g. "+" button to create item in group).</summary>
    public object? Action
    {
        get => GetValue(ActionProperty);
        set => SetValue(ActionProperty, value);
    }

    /// <summary>Template for <see cref="Action"/>.</summary>
    public DataTemplate? ActionTemplate
    {
        get => (DataTemplate?)GetValue(ActionTemplateProperty);
        set => SetValue(ActionTemplateProperty, value);
    }

    protected override bool IsItemItsOwnContainerOverride(object item) =>
        item is NavTreeItem || item is NavTreeGroup;

    protected override DependencyObject GetContainerForItemOverride() =>
        new NavTreeItem();

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (GetTemplateChild("PART_HeaderButton") is ButtonBase headerButton)
        {
            headerButton.Click -= OnHeaderButtonClick;
            headerButton.Click += OnHeaderButtonClick;
        }
    }

    private void OnHeaderButtonClick(object sender, RoutedEventArgs e)
    {
        IsExpanded = !IsExpanded;
    }
}
