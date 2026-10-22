using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace CleanTheme.Wpf.Controls;

/// <summary>
/// A category tab in a <see cref="SearchableSettingsView"/> (e.g. "My details", "Notifications").
/// </summary>
public class SettingsCategory : HeaderedItemsControl
{
    public static readonly RoutedEvent SelectedEvent =
        Selector.SelectedEvent.AddOwner(typeof(SettingsCategory));

    public static readonly RoutedEvent UnselectedEvent =
        Selector.UnselectedEvent.AddOwner(typeof(SettingsCategory));

    public static readonly DependencyProperty BadgeProperty = DependencyProperty.Register(
        nameof(Badge), typeof(object), typeof(SettingsCategory),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty IsMatchProperty = DependencyProperty.Register(
        nameof(IsMatch), typeof(bool), typeof(SettingsCategory),
        new FrameworkPropertyMetadata(true));

    public static readonly DependencyProperty IsSelectedProperty =
        Selector.IsSelectedProperty.AddOwner(
            typeof(SettingsCategory),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                OnIsSelectedChanged));

    static SettingsCategory()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SettingsCategory), new FrameworkPropertyMetadata(typeof(SettingsCategory)));
        FocusableProperty.OverrideMetadata(typeof(SettingsCategory), new FrameworkPropertyMetadata(true));
    }

    private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SettingsCategory category)
        {
            bool isSelected = (bool)e.NewValue;
            var routedEvent = isSelected ? SelectedEvent : UnselectedEvent;
            category.RaiseEvent(new RoutedEventArgs(routedEvent, category));

            if (UIElementAutomationPeer.FromElement(category) is SettingsCategoryAutomationPeer peer)
            {
                peer.RaisePropertyChangedEvent(
                    SelectionItemPatternIdentifiers.IsSelectedProperty,
                    (bool)e.OldValue,
                    isSelected);
            }
        }
    }

    /// <summary>Optional badge count or label shown next to the category title.</summary>
    public object? Badge
    {
        get => GetValue(BadgeProperty);
        set => SetValue(BadgeProperty, value);
    }

    /// <summary>Whether this category tab is currently selected in the parent settings view.</summary>
    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    /// <summary>Whether this category or any of its children match the active filter.</summary>
    public bool IsMatch
    {
        get => (bool)GetValue(IsMatchProperty);
        set => SetValue(IsMatchProperty, value);
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        if (!e.Handled)
        {
            SelectThis();
            Focus();
            e.Handled = true;
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Handled) return;

        if (e.Key is Key.Enter or Key.Space)
        {
            SelectThis();
            e.Handled = true;
        }
        else if (e.Key is Key.Right or Key.Down)
        {
            MoveFocusToAdjacentCategory(forward: true);
            e.Handled = true;
        }
        else if (e.Key is Key.Left or Key.Up)
        {
            MoveFocusToAdjacentCategory(forward: false);
            e.Handled = true;
        }
    }

    private void SelectThis()
    {
        IsSelected = true;
        if (ItemsControl.ItemsControlFromItemContainer(this) is Selector owner)
        {
            owner.SelectedItem = owner.ItemContainerGenerator.ItemFromContainer(this) ?? this;
        }
    }

    private void MoveFocusToAdjacentCategory(bool forward)
    {
        if (ItemsControl.ItemsControlFromItemContainer(this) is Selector selector)
        {
            int index = selector.ItemContainerGenerator.IndexFromContainer(this);
            if (index < 0) index = selector.Items.IndexOf(this);

            if (index >= 0)
            {
                int count = selector.Items.Count;
                int nextIndex = forward ? index + 1 : index - 1;
                while (nextIndex >= 0 && nextIndex < count)
                {
                    var item = selector.Items[nextIndex];
                    var container = selector.ItemContainerGenerator.ContainerFromIndex(nextIndex) as SettingsCategory
                                    ?? item as SettingsCategory;

                    if (container != null && container.IsMatch && container.Visibility == Visibility.Visible)
                    {
                        container.Focus();
                        container.SelectThis();
                        break;
                    }
                    nextIndex = forward ? nextIndex + 1 : nextIndex - 1;
                }
            }
        }
    }

    protected override AutomationPeer OnCreateAutomationPeer() => new SettingsCategoryAutomationPeer(this);

    /// <summary>Filters this category and its contained groups and rows.</summary>
    public bool Filter(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            foreach (var item in Items)
            {
                if (item is SettingsGroup group) group.Filter("");
                else if (item is SettingRow row) row.Filter("");
            }
            IsMatch = true;
            return true;
        }

        int matchCount = 0;
        bool catHeaderMatches = Header?.ToString()?.Contains(query, StringComparison.OrdinalIgnoreCase) == true;

        foreach (var item in Items)
        {
            if (item is SettingsGroup group)
            {
                if (catHeaderMatches)
                {
                    group.Filter("");
                    group.IsMatch = true;
                    matchCount++;
                }
                else if (group.Filter(query))
                {
                    matchCount++;
                }
            }
            else if (item is SettingRow row)
            {
                if (catHeaderMatches || row.Filter(query))
                {
                    row.IsMatch = true;
                    matchCount++;
                }
            }
            else
            {
                matchCount++;
            }
        }

        IsMatch = matchCount > 0;
        return IsMatch;
    }
}

/// <summary>Automation peer for <see cref="SettingsCategory"/> presenting it as a TabItem with selection item pattern.</summary>
public class SettingsCategoryAutomationPeer : FrameworkElementAutomationPeer, ISelectionItemProvider
{
    public SettingsCategoryAutomationPeer(SettingsCategory owner) : base(owner) { }

    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.TabItem;

    protected override string GetNameCore()
    {
        var name = AutomationProperties.GetName(Owner);
        if (!string.IsNullOrEmpty(name)) return name;
        if (Owner is SettingsCategory { Header: not null } cat) return cat.Header.ToString() ?? string.Empty;
        return base.GetNameCore();
    }

    public override object? GetPattern(PatternInterface patternInterface)
    {
        if (patternInterface == PatternInterface.SelectionItem)
        {
            return this;
        }
        return base.GetPattern(patternInterface);
    }

    bool ISelectionItemProvider.IsSelected => ((SettingsCategory)Owner).IsSelected;

    IRawElementProviderSimple? ISelectionItemProvider.SelectionContainer
    {
        get
        {
            if (ItemsControl.ItemsControlFromItemContainer(Owner) is Selector selector &&
                CreatePeerForElement(selector) is AutomationPeer peer)
            {
                return ProviderFromPeer(peer);
            }
            return null;
        }
    }

    void ISelectionItemProvider.Select()
    {
        if (Owner is SettingsCategory cat)
        {
            cat.IsSelected = true;
            if (ItemsControl.ItemsControlFromItemContainer(cat) is Selector owner)
            {
                owner.SelectedItem = owner.ItemContainerGenerator.ItemFromContainer(cat) ?? cat;
            }
        }
    }

    void ISelectionItemProvider.AddToSelection() => ((ISelectionItemProvider)this).Select();

    void ISelectionItemProvider.RemoveFromSelection()
    {
        if (Owner is SettingsCategory cat)
        {
            cat.IsSelected = false;
        }
    }
}
