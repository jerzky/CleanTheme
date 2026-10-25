using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace CleanTheme.Wpf.Controls;

/// <summary>
/// A selectable navigation item in a <see cref="NavTree"/> or <see cref="NavTreeGroup"/>.
/// Supports hierarchical sub-items (tree branch lines and expander), vector icon, label, badge,
/// active indicator bar, hover action buttons, and ICommand execution.
/// </summary>
public class NavTreeItem : HeaderedItemsControl, ICommandSource
{
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon), typeof(Geometry), typeof(NavTreeItem),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty IconElementProperty = DependencyProperty.Register(
        nameof(IconElement), typeof(object), typeof(NavTreeItem),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty BadgeProperty = DependencyProperty.Register(
        nameof(Badge), typeof(object), typeof(NavTreeItem),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty HoverActionsProperty = DependencyProperty.Register(
        nameof(HoverActions), typeof(object), typeof(NavTreeItem),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty HoverActionsTemplateProperty = DependencyProperty.Register(
        nameof(HoverActionsTemplate), typeof(DataTemplate), typeof(NavTreeItem),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
        nameof(IsSelected), typeof(bool), typeof(NavTreeItem),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsSelectedChanged));

    public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(
        nameof(IsActive), typeof(bool), typeof(NavTreeItem),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(
        nameof(IsExpanded), typeof(bool), typeof(NavTreeItem),
        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsExpandedChanged));

    public static readonly DependencyProperty IsSubItemProperty = DependencyProperty.Register(
        nameof(IsSubItem), typeof(bool), typeof(NavTreeItem),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
        nameof(Command), typeof(ICommand), typeof(NavTreeItem),
        new PropertyMetadata(null, OnCommandChanged));

    public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
        nameof(CommandParameter), typeof(object), typeof(NavTreeItem),
        new PropertyMetadata(null));

    public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register(
        nameof(CommandTarget), typeof(IInputElement), typeof(NavTreeItem),
        new PropertyMetadata(null));

    public static readonly RoutedEvent SelectedEvent = EventManager.RegisterRoutedEvent(
        nameof(Selected), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NavTreeItem));

    public static readonly RoutedEvent UnselectedEvent = EventManager.RegisterRoutedEvent(
        nameof(Unselected), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NavTreeItem));

    public static readonly RoutedEvent ExpandedEvent = EventManager.RegisterRoutedEvent(
        nameof(Expanded), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NavTreeItem));

    public static readonly RoutedEvent CollapsedEvent = EventManager.RegisterRoutedEvent(
        nameof(Collapsed), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NavTreeItem));

    static NavTreeItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(NavTreeItem), new FrameworkPropertyMetadata(typeof(NavTreeItem)));
    }

    /// <summary>Vector geometry outline for the item icon.</summary>
    public Geometry? Icon
    {
        get => (Geometry?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Custom visual element for the icon (e.g. image, text emoji, status badge).</summary>
    public object? IconElement
    {
        get => GetValue(IconElementProperty);
        set => SetValue(IconElementProperty, value);
    }

    /// <summary>Optional badge content (e.g. unread count or tag).</summary>
    public object? Badge
    {
        get => GetValue(BadgeProperty);
        set => SetValue(BadgeProperty, value);
    }

    /// <summary>Contextual action buttons displayed on hover/focus (e.g. close, delete, edit).</summary>
    public object? HoverActions
    {
        get => GetValue(HoverActionsProperty);
        set => SetValue(HoverActionsProperty, value);
    }

    /// <summary>Template for <see cref="HoverActions"/>.</summary>
    public DataTemplate? HoverActionsTemplate
    {
        get => (DataTemplate?)GetValue(HoverActionsTemplateProperty);
        set => SetValue(HoverActionsTemplateProperty, value);
    }

    /// <summary>Gets or sets whether this item is currently selected.</summary>
    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    /// <summary>Gets or sets whether this item is active (shows accent indicator bar).</summary>
    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    /// <summary>Gets or sets whether child sub-items are expanded to view.</summary>
    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    /// <summary>Gets or sets whether this item is a nested child/sub-item (displays branch guide connector line).</summary>
    public bool IsSubItem
    {
        get => (bool)GetValue(IsSubItemProperty);
        set => SetValue(IsSubItemProperty, value);
    }

    /// <summary>Command executed when this item is clicked or selected.</summary>
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>Parameter passed to <see cref="Command"/>.</summary>
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    /// <summary>Target element for the command.</summary>
    public IInputElement? CommandTarget
    {
        get => (IInputElement?)GetValue(CommandTargetProperty);
        set => SetValue(CommandTargetProperty, value);
    }

    public event RoutedEventHandler Selected
    {
        add => AddHandler(SelectedEvent, value);
        remove => RemoveHandler(SelectedEvent, value);
    }

    public event RoutedEventHandler Unselected
    {
        add => AddHandler(UnselectedEvent, value);
        remove => RemoveHandler(UnselectedEvent, value);
    }

    /// <summary>Raised when child items are expanded.</summary>
    public event RoutedEventHandler Expanded
    {
        add => AddHandler(ExpandedEvent, value);
        remove => RemoveHandler(ExpandedEvent, value);
    }

    /// <summary>Raised when child items are collapsed.</summary>
    public event RoutedEventHandler Collapsed
    {
        add => AddHandler(CollapsedEvent, value);
        remove => RemoveHandler(CollapsedEvent, value);
    }

    protected override bool IsItemItsOwnContainerOverride(object item) =>
        item is NavTreeItem;

    protected override DependencyObject GetContainerForItemOverride() =>
        new NavTreeItem();

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);
        if (element is NavTreeItem childItem)
        {
            childItem.IsSubItem = true;
        }
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (GetTemplateChild("PART_Expander") is ToggleButton expander)
        {
            expander.Click -= OnExpanderClick;
            expander.Click += OnExpanderClick;
        }
    }

    private void OnExpanderClick(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
    }

    protected override void OnVisualParentChanged(DependencyObject oldParent)
    {
        base.OnVisualParentChanged(oldParent);
        UpdateIsSubItem();
    }

    private void UpdateIsSubItem()
    {
        var current = VisualTreeHelper.GetParent(this) ?? LogicalTreeHelper.GetParent(this);
        while (current != null)
        {
            if (current is NavTreeItem)
            {
                IsSubItem = true;
                return;
            }
            if (current is NavTree || current is NavTreeGroup)
            {
                break;
            }
            current = VisualTreeHelper.GetParent(current) ?? LogicalTreeHelper.GetParent(current);
        }
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);
        if (!e.Handled)
        {
            SelectThis();
            ExecuteCommand();
            e.Handled = true;
        }
    }

    protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
    {
        base.OnMouseDoubleClick(e);
        if (!e.Handled && HasItems && e.ChangedButton == MouseButton.Left)
        {
            IsExpanded = !IsExpanded;
            e.Handled = true;
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Handled) return;

        if (e.Key == Key.Enter || e.Key == Key.Space)
        {
            SelectThis();
            ExecuteCommand();
            e.Handled = true;
        }
        else if (e.Key == Key.Right && HasItems && !IsExpanded)
        {
            IsExpanded = true;
            e.Handled = true;
        }
        else if (e.Key == Key.Left && HasItems && IsExpanded)
        {
            IsExpanded = false;
            e.Handled = true;
        }
    }

    public void SelectThis()
    {
        IsSelected = true;
        FindParentNavTree(this)?.NotifyItemSelected(this);
    }

    private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NavTreeItem item)
        {
            bool isSelected = (bool)e.NewValue;
            if (isSelected)
            {
                item.RaiseEvent(new RoutedEventArgs(SelectedEvent, item));
            }
            else
            {
                item.RaiseEvent(new RoutedEventArgs(UnselectedEvent, item));
            }
        }
    }

    private static void OnIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NavTreeItem item)
        {
            bool isExpanded = (bool)e.NewValue;
            item.RaiseEvent(new RoutedEventArgs(isExpanded ? ExpandedEvent : CollapsedEvent, item));
        }
    }

    private static NavTree? FindParentNavTree(DependencyObject element)
    {
        var current = VisualTreeHelper.GetParent(element) ?? LogicalTreeHelper.GetParent(element);
        while (current != null)
        {
            if (current is NavTree tree) return tree;
            current = VisualTreeHelper.GetParent(current) ?? LogicalTreeHelper.GetParent(current);
        }
        return null;
    }

    private void ExecuteCommand()
    {
        if (Command is null) return;
        var param = CommandParameter;
        if (Command is RoutedCommand routed)
        {
            if (routed.CanExecute(param, CommandTarget ?? this))
            {
                routed.Execute(param, CommandTarget ?? this);
            }
        }
        else if (Command.CanExecute(param))
        {
            Command.Execute(param);
        }
    }

    private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NavTreeItem item)
        {
            if (e.OldValue is ICommand oldCommand)
            {
                oldCommand.CanExecuteChanged -= item.OnCanExecuteChanged;
            }
            if (e.NewValue is ICommand newCommand)
            {
                newCommand.CanExecuteChanged += item.OnCanExecuteChanged;
                item.UpdateCanExecute();
            }
        }
    }

    private void OnCanExecuteChanged(object? sender, EventArgs e) => UpdateCanExecute();

    private void UpdateCanExecute()
    {
        if (Command is not null)
        {
            var param = CommandParameter;
            bool canExecute = Command is RoutedCommand routed
                ? routed.CanExecute(param, CommandTarget ?? this)
                : Command.CanExecute(param);
            IsEnabled = canExecute;
        }
    }

    protected override AutomationPeer OnCreateAutomationPeer() =>
        new NavTreeItemAutomationPeer(this);
}

/// <summary>
/// Exposes <see cref="NavTreeItem"/> to UI Automation as a TreeItem supporting Expand/Collapse and Selection.
/// </summary>
public class NavTreeItemAutomationPeer : FrameworkElementAutomationPeer, IExpandCollapseProvider, ISelectionItemProvider
{
    public NavTreeItemAutomationPeer(NavTreeItem owner) : base(owner) { }

    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.TreeItem;

    protected override string GetClassNameCore() => nameof(NavTreeItem);

    protected override string GetNameCore()
    {
        if (Owner is NavTreeItem item)
        {
            if (item.Header is string str && !string.IsNullOrWhiteSpace(str)) return str;
            if (item.Header != null) return item.Header.ToString() ?? nameof(NavTreeItem);
        }
        return base.GetNameCore() ?? nameof(NavTreeItem);
    }

    public override object? GetPattern(PatternInterface patternInterface)
    {
        if (patternInterface == PatternInterface.ExpandCollapse && ((NavTreeItem)Owner).HasItems)
        {
            return this;
        }
        if (patternInterface == PatternInterface.SelectionItem)
        {
            return this;
        }
        return base.GetPattern(patternInterface);
    }

    public ExpandCollapseState ExpandCollapseState => ((NavTreeItem)Owner).IsExpanded
        ? ExpandCollapseState.Expanded
        : ExpandCollapseState.Collapsed;

    public void Expand() => ((NavTreeItem)Owner).IsExpanded = true;
    public void Collapse() => ((NavTreeItem)Owner).IsExpanded = false;

    public bool IsSelected => ((NavTreeItem)Owner).IsSelected;
    public IRawElementProviderSimple? SelectionContainer => null;
    public void AddToSelection() => ((NavTreeItem)Owner).SelectThis();
    public void RemoveFromSelection() => ((NavTreeItem)Owner).IsSelected = false;
    public void Select() => ((NavTreeItem)Owner).SelectThis();
}
