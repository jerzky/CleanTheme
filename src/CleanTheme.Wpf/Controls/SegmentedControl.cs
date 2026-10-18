using System.Windows;
using System.Windows.Controls;

namespace CleanTheme.Wpf.Controls;

/// <summary>
/// A row of joined choices with exactly one selected, e.g. a time scale. Bind <c>ItemsSource</c> and <c>SelectedItem</c>
/// (or <c>SelectedValue</c> with <c>SelectedValuePath</c>) as with a ListBox.
/// </summary>
public sealed class SegmentedControl : ListBox
{
    static SegmentedControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SegmentedControl), new FrameworkPropertyMetadata(typeof(SegmentedControl)));
        SelectionModeProperty.OverrideMetadata(typeof(SegmentedControl), new FrameworkPropertyMetadata(SelectionMode.Single));
    }

    protected override DependencyObject GetContainerForItemOverride() => new SegmentedItem();

    protected override bool IsItemItsOwnContainerOverride(object item) => item is SegmentedItem;
}

/// <summary>One choice in a <see cref="SegmentedControl"/>.</summary>
public sealed class SegmentedItem : ListBoxItem
{
    static SegmentedItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SegmentedItem), new FrameworkPropertyMetadata(typeof(SegmentedItem)));
    }
}
