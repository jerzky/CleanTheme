using System.Windows;
using System.Windows.Controls;

namespace CleanTheme.Wpf.Controls;

/// <summary>
/// A two-column settings row: label, description, and optional required indicator on the left,
/// interactive editor control on the right.
/// </summary>
public class SettingRow : HeaderedContentControl
{
    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
        nameof(Description), typeof(string), typeof(SettingRow),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty IsRequiredProperty = DependencyProperty.Register(
        nameof(IsRequired), typeof(bool), typeof(SettingRow),
        new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty KeywordsProperty = DependencyProperty.Register(
        nameof(Keywords), typeof(string), typeof(SettingRow),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty HeaderWidthProperty = DependencyProperty.Register(
        nameof(HeaderWidth), typeof(GridLength), typeof(SettingRow),
        new FrameworkPropertyMetadata(new GridLength(260)));

    public static readonly DependencyProperty IsMatchProperty = DependencyProperty.Register(
        nameof(IsMatch), typeof(bool), typeof(SettingRow),
        new FrameworkPropertyMetadata(true));

    static SettingRow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SettingRow), new FrameworkPropertyMetadata(typeof(SettingRow)));
    }

    /// <summary>Helper description text below the label.</summary>
    public string? Description
    {
        get => (string?)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>Whether an accent asterisk '*' is displayed next to the label.</summary>
    public bool IsRequired
    {
        get => (bool)GetValue(IsRequiredProperty);
        set => SetValue(IsRequiredProperty, value);
    }

    /// <summary>Optional search keywords for filtering.</summary>
    public string? Keywords
    {
        get => (string?)GetValue(KeywordsProperty);
        set => SetValue(KeywordsProperty, value);
    }

    /// <summary>Width of the label column (default 260).</summary>
    public GridLength HeaderWidth
    {
        get => (GridLength)GetValue(HeaderWidthProperty);
        set => SetValue(HeaderWidthProperty, value);
    }

    /// <summary>Whether this row matches the active search query.</summary>
    public bool IsMatch
    {
        get => (bool)GetValue(IsMatchProperty);
        set => SetValue(IsMatchProperty, value);
    }

    /// <summary>Filters this row against the specified search query.</summary>
    public bool Filter(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            IsMatch = true;
            return true;
        }

        string headerStr = Header?.ToString() ?? "";
        string descStr = Description ?? "";
        string keywordsStr = Keywords ?? "";

        bool match = headerStr.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                     descStr.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                     keywordsStr.Contains(query, StringComparison.OrdinalIgnoreCase);

        IsMatch = match;
        return match;
    }
}
