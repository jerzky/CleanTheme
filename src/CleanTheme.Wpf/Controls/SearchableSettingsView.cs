using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CleanTheme.Wpf.Controls;

/// <summary>
/// A searchable settings view with a header title, quick search filter (with Ctrl+K shortcut),
/// horizontal category navigation tabs, and structured settings groups.
/// </summary>
public class SearchableSettingsView : ListBox
{
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title), typeof(string), typeof(SearchableSettingsView),
        new FrameworkPropertyMetadata("Settings"));

    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
        nameof(Description), typeof(string), typeof(SearchableSettingsView),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty SearchTextProperty = DependencyProperty.Register(
        nameof(SearchText), typeof(string), typeof(SearchableSettingsView),
        new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSearchTextChanged));

    public static readonly DependencyProperty SearchPlaceholderProperty = DependencyProperty.Register(
        nameof(SearchPlaceholder), typeof(string), typeof(SearchableSettingsView),
        new FrameworkPropertyMetadata("Search settings (Ctrl+K)"));

    private static readonly DependencyPropertyKey HasMatchesPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(HasMatches), typeof(bool), typeof(SearchableSettingsView),
        new FrameworkPropertyMetadata(true));

    public static readonly DependencyProperty HasMatchesProperty = HasMatchesPropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey IsSearchingPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(IsSearching), typeof(bool), typeof(SearchableSettingsView),
        new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty IsSearchingProperty = IsSearchingPropertyKey.DependencyProperty;

    private TextBox? _searchBox;

    static SearchableSettingsView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchableSettingsView), new FrameworkPropertyMetadata(typeof(SearchableSettingsView)));
        SelectionModeProperty.OverrideMetadata(typeof(SearchableSettingsView), new FrameworkPropertyMetadata(SelectionMode.Single));
    }

    public SearchableSettingsView()
    {
        Loaded += OnLoaded;
    }

    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);
        if (SelectedIndex < 0 && Items.Count > 0)
        {
            SelectedIndex = 0;
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (SelectedIndex < 0 && Items.Count > 0)
        {
            SelectedIndex = 0;
        }
    }

    /// <summary>The title displayed at the top left (default "Settings").</summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Optional subtitle or description displayed below the title.</summary>
    public string? Description
    {
        get => (string?)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>Active search filter text.</summary>
    public string SearchText
    {
        get => (string)GetValue(SearchTextProperty);
        set => SetValue(SearchTextProperty, value);
    }

    /// <summary>Placeholder hint for the search box.</summary>
    public string SearchPlaceholder
    {
        get => (string)GetValue(SearchPlaceholderProperty);
        set => SetValue(SearchPlaceholderProperty, value);
    }

    /// <summary>Whether any settings matched the active search filter.</summary>
    public bool HasMatches => (bool)GetValue(HasMatchesProperty);

    /// <summary>Whether a search query is currently active.</summary>
    public bool IsSearching => (bool)GetValue(IsSearchingProperty);

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (GetTemplateChild("PART_SearchBox") is TextBox searchBox)
        {
            _searchBox = searchBox;
        }
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        if (e.Key == Key.K && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            if (_searchBox is not null)
            {
                _searchBox.Focus();
                _searchBox.SelectAll();
                e.Handled = true;
            }
        }
    }

    private static void OnSearchTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SearchableSettingsView view)
        {
            view.ApplyFilter((string)e.NewValue);
        }
    }

    private void ApplyFilter(string query)
    {
        bool isSearching = !string.IsNullOrWhiteSpace(query);
        SetValue(IsSearchingPropertyKey, isSearching);

        int matchCount = 0;
        foreach (var item in Items)
        {
            if (item is SettingsCategory category)
            {
                if (category.Filter(query)) matchCount++;
            }
            else if (item is SettingsGroup group)
            {
                if (group.Filter(query)) matchCount++;
            }
            else if (item is SettingRow row)
            {
                if (row.Filter(query)) matchCount++;
            }
        }

        if (isSearching && (SelectedItem is not SettingsCategory currentCat || !currentCat.IsMatch))
        {
            foreach (var item in Items)
            {
                if (item is SettingsCategory cat && cat.IsMatch)
                {
                    SelectedItem = cat;
                    break;
                }
            }
        }

        SetValue(HasMatchesPropertyKey, !isSearching || matchCount > 0);
    }

    protected override DependencyObject GetContainerForItemOverride() => new SettingsCategory();

    protected override bool IsItemItsOwnContainerOverride(object item) => item is SettingsCategory;
}
