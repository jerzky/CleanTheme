using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace CleanTheme.Wpf.Controls;

/// <summary>
/// A text box for filtering: magnifier icon, <see cref="Placeholder"/> while empty, and a clear button (or Escape) once
/// there is text. Bind <c>Text</c> as with any TextBox.
/// </summary>
[TemplatePart(Name = ClearButtonPart, Type = typeof(ButtonBase))]
public sealed class SearchBox : TextBox
{
    private const string ClearButtonPart = "PART_ClearButton";

    public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(
        nameof(Placeholder), typeof(string), typeof(SearchBox), new FrameworkPropertyMetadata("Search"));

    private ButtonBase? _clearButton;

    static SearchBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchBox), new FrameworkPropertyMetadata(typeof(SearchBox)));
    }

    /// <summary>Hint shown while the box is empty. "Search" by default.</summary>
    public string? Placeholder
    {
        get => (string?)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public override void OnApplyTemplate()
    {
        if (_clearButton != null)
        {
            _clearButton.Click -= OnClearClick;
        }
        base.OnApplyTemplate();
        _clearButton = GetTemplateChild(ClearButtonPart) as ButtonBase;
        if (_clearButton != null)
        {
            _clearButton.Click += OnClearClick;
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        if (e.Key == Key.Escape && Text.Length > 0)
        {
            Clear();
            e.Handled = true;
            return;
        }
        base.OnKeyDown(e);
    }

    private void OnClearClick(object sender, RoutedEventArgs e)
    {
        Clear();
        Focus();
    }
}
