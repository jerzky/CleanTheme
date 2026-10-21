using System.Windows;
using System.Windows.Controls;

namespace CleanTheme.Wpf.Controls;

/// <summary>
/// A section grouping related <see cref="SettingRow"/> items with a title, description,
/// and optional right-aligned action buttons (e.g. Save, Cancel).
/// </summary>
public class SettingsGroup : HeaderedItemsControl
{
    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
        nameof(Description), typeof(string), typeof(SettingsGroup),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty ActionsProperty = DependencyProperty.Register(
        nameof(Actions), typeof(object), typeof(SettingsGroup),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty ActionsTemplateProperty = DependencyProperty.Register(
        nameof(ActionsTemplate), typeof(DataTemplate), typeof(SettingsGroup),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty IsMatchProperty = DependencyProperty.Register(
        nameof(IsMatch), typeof(bool), typeof(SettingsGroup),
        new FrameworkPropertyMetadata(true));

    static SettingsGroup()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SettingsGroup), new FrameworkPropertyMetadata(typeof(SettingsGroup)));
    }

    /// <summary>Helper description beneath the group title.</summary>
    public string? Description
    {
        get => (string?)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>Right-aligned actions (e.g. Cancel and Save buttons).</summary>
    public object? Actions
    {
        get => GetValue(ActionsProperty);
        set => SetValue(ActionsProperty, value);
    }

    /// <summary>Template for <see cref="Actions"/>.</summary>
    public DataTemplate? ActionsTemplate
    {
        get => (DataTemplate?)GetValue(ActionsTemplateProperty);
        set => SetValue(ActionsTemplateProperty, value);
    }

    /// <summary>Whether this group or any of its child rows match the active filter.</summary>
    public bool IsMatch
    {
        get => (bool)GetValue(IsMatchProperty);
        set => SetValue(IsMatchProperty, value);
    }

    /// <summary>Filters this group and its child rows.</summary>
    public bool Filter(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            foreach (var item in Items)
            {
                if (item is SettingRow row) row.Filter("");
            }
            IsMatch = true;
            return true;
        }

        int matchCount = 0;
        bool groupHeaderMatches = Header?.ToString()?.Contains(query, StringComparison.OrdinalIgnoreCase) == true;

        foreach (var item in Items)
        {
            if (item is SettingRow row)
            {
                bool rowMatches = groupHeaderMatches || row.Filter(query);
                if (rowMatches)
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
