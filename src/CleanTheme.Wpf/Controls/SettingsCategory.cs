using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace CleanTheme.Wpf.Controls;

/// <summary>
/// A category tab in a <see cref="SearchableSettingsView"/> (e.g. "My details", "Notifications").
/// </summary>
public class SettingsCategory : HeaderedItemsControl
{
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
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal));

    static SettingsCategory()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SettingsCategory), new FrameworkPropertyMetadata(typeof(SettingsCategory)));
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
            IsSelected = true;
            Focus();
            e.Handled = true;
        }
    }

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
