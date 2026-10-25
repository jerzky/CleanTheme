using System.Windows;
using System.Windows.Controls;

namespace CleanTheme.Wpf.Controls;

/// <summary>
/// A hierarchical navigation tree control for sidebars, cockpits, and drawers.
/// Supports collapsible sections (<see cref="NavTreeGroup"/>) and selectable leaf items (<see cref="NavTreeItem"/>).
/// </summary>
public class NavTree : ItemsControl
{
    public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
        nameof(SelectedItem), typeof(object), typeof(NavTree),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

    public static readonly RoutedEvent SelectedItemChangedEvent = EventManager.RegisterRoutedEvent(
        nameof(SelectedItemChanged), RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<object?>), typeof(NavTree));

    static NavTree()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(NavTree), new FrameworkPropertyMetadata(typeof(NavTree)));
        FocusableProperty.OverrideMetadata(typeof(NavTree), new FrameworkPropertyMetadata(false));
    }

    /// <summary>Gets or sets the currently selected item or data model in the tree.</summary>
    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /// <summary>Raised when <see cref="SelectedItem"/> changes.</summary>
    public event RoutedPropertyChangedEventHandler<object?> SelectedItemChanged
    {
        add => AddHandler(SelectedItemChangedEvent, value);
        remove => RemoveHandler(SelectedItemChangedEvent, value);
    }

    protected override bool IsItemItsOwnContainerOverride(object item) =>
        item is NavTreeGroup || item is NavTreeItem;

    protected override DependencyObject GetContainerForItemOverride() =>
        new NavTreeItem();

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NavTree tree)
        {
            tree.OnSelectedItemChanged(e.OldValue, e.NewValue);
        }
    }

    protected virtual void OnSelectedItemChanged(object? oldValue, object? newValue)
    {
        UpdateSelectionState(this, newValue);
        var args = new RoutedPropertyChangedEventArgs<object?>(oldValue, newValue, SelectedItemChangedEvent);
        RaiseEvent(args);
    }

    internal void NotifyItemSelected(NavTreeItem selectedItem)
    {
        var targetModel = selectedItem.DataContext ?? selectedItem;
        SelectedItem = targetModel;
    }

    private static void UpdateSelectionState(ItemsControl parent, object? target)
    {
        foreach (var item in parent.Items)
        {
            var container = parent.ItemContainerGenerator.ContainerFromItem(item) ?? (item as DependencyObject);
            if (container is NavTreeItem navItem)
            {
                bool isTarget = (navItem == target) || (navItem.DataContext != null && navItem.DataContext == target);
                navItem.IsSelected = isTarget;
                if (navItem.HasItems)
                {
                    UpdateSelectionState(navItem, target);
                }
            }
            else if (container is NavTreeGroup group)
            {
                UpdateSelectionState(group, target);
            }
        }
    }
}
