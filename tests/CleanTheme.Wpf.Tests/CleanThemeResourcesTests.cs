using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace CleanTheme.Wpf.Tests;

public sealed class CleanThemeResourcesTests
{
    // The keys consumers reference. Renaming or removing one is a breaking change.
    private static readonly string[] PublicKeys =
    [
        "AppBackgroundColor", "AppBackground", "Surface", "SurfaceSunken", "Hover", "Pressed", "Border", "BorderStrong", "Divider",
        "Text", "TextMuted", "TextSubtle", "TextLog",
        "Accent", "AccentHover", "AccentPressed", "AccentSoft",
        "Positive", "PositiveSoft", "Warning", "WarningSoft", "Negative", "NegativeSoft", "NegativeBorder", "NeutralSoft",
        "UiFont", "MonoFont",
        "CardTitle", "FieldLabel", "MetricLabel", "MetricValue",
        "CleanWindow", "Card", "Pill",
        "ButtonBaseStyle", "PrimaryButton", "DangerButton", "GhostButton", "SegmentButton", "RowButton",
        "NumericTextBox", "NumericHeader", "NumericCell", "LogList",
        "ScrollBarThumb", "ScrollBarPageButton", "VerticalScrollBarTemplate", "HorizontalScrollBarTemplate",
        "CleanContextMenu", "CleanMenuItem",
    ];

    private static readonly Type[] ImplicitlyStyled =
    [
        typeof(Button), typeof(TextBox), typeof(ComboBox), typeof(ComboBoxItem),
        typeof(DataGrid), typeof(DataGridColumnHeader), typeof(DataGridRow), typeof(DataGridCell),
        typeof(ScrollBar), typeof(ScrollViewer),
    ];

    [Fact]
    public void Theme_defines_every_public_key() => Sta.Run(() =>
    {
        var theme = new CleanThemeResources();

        var missing = PublicKeys.Where(key => !theme.Contains(key)).ToArray();

        Assert.Empty(missing);
    });

    [Fact]
    public void Theme_defines_implicit_styles_for_stock_controls() => Sta.Run(() =>
    {
        var theme = new CleanThemeResources();

        var missing = ImplicitlyStyled.Where(type => !theme.Contains(type)).Select(type => type.Name).ToArray();

        Assert.Empty(missing);
    });

    [Fact]
    public void Every_style_applies_to_its_target_type() => Sta.Run(() =>
    {
        var theme = new CleanThemeResources();

        foreach (var style in theme.Values.OfType<Style>())
        {
            // ButtonBaseStyle targets an abstract type; Button stands in for it.
            var targetType = style.TargetType == typeof(ButtonBase) ? typeof(Button) : style.TargetType;
            if (targetType == typeof(Window))
            {
                continue; // Creating a Window needs an Application; the setters are plain values.
            }

            var element = (FrameworkElement)Activator.CreateInstance(targetType)!; // All targets have public parameterless constructors.
            element.Resources.MergedDictionaries.Add(theme);
            element.Style = style;
            element.ApplyTemplate();
            element.Measure(new Size(200, 200));
        }
    });

    [Fact]
    public void SegmentButton_highlights_when_checked() => Sta.Run(() =>
    {
        var theme = new CleanThemeResources();
        var button = new ToggleButton { Style = (Style)theme["SegmentButton"] };
        button.Resources.MergedDictionaries.Add(theme);

        button.IsChecked = true;

        Assert.Same(theme["AccentSoft"], button.Background);
        Assert.Same(theme["Accent"], button.Foreground);
    });

    [Fact]
    public void Theme_uri_points_at_the_bundled_dictionary() => Sta.Run(() =>
    {
        var theme = new ResourceDictionary { Source = CleanThemeResources.ThemeUri };

        Assert.IsType<SolidColorBrush>(theme["Accent"]);
    });
}
