using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Threading;

namespace CleanTheme.Wpf;

/// <summary>
/// The CleanTheme resource dictionary, for merging into application resources:
/// <c>&lt;ResourceDictionary.MergedDictionaries&gt;&lt;clean:CleanThemeResources Mode="System" /&gt;&lt;/ResourceDictionary.MergedDictionaries&gt;</c>.
/// It holds the styles plus the active palette, which it swaps when <see cref="ThemeManager.Mode"/> changes.
/// </summary>
/// <remarks>
/// Merge it into <c>Application.Resources</c> (not only a window's): context menus and tool tips live outside the
/// window's element tree and find the palette there.
/// </remarks>
[SuppressMessage("Design", "CA1010:Generic interface should also be implemented", Justification = "The collection surface is WPF's ResourceDictionary; this type only sets up merged dictionaries.")]
public sealed class CleanThemeResources : ResourceDictionary, IThemeTarget
{
    private readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;
    private CleanThemeMode _applied;

    /// <summary>Pack URI of the styles dictionary (light palette included), for merging by <c>Source</c> instead of by type.</summary>
    public static Uri ThemeUri { get; } = new("pack://application:,,,/CleanTheme.Wpf;component/Themes/CleanTheme.xaml", UriKind.Absolute);

    /// <summary>Loads the styles and the palette for <see cref="ThemeManager.ActualMode"/>.</summary>
    public CleanThemeResources()
    {
        MergedDictionaries.Add(new ResourceDictionary { Source = ThemeUri });
        _applied = ThemeManager.ActualMode;
        MergedDictionaries.Add(new ResourceDictionary { Source = PaletteUri(_applied) });
        ThemeManager.Register(this);
    }

    /// <summary>The app-wide mode; setting it here (e.g. <c>Mode="System"</c> in XAML) sets <see cref="ThemeManager.Mode"/>.</summary>
    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "An instance property so XAML can set it on the dictionary.")]
    public CleanThemeMode Mode
    {
        get => ThemeManager.Mode;
        set => ThemeManager.Mode = value;
    }

    /// <summary>Pack URI of the light or dark palette.</summary>
    public static Uri PaletteUri(CleanThemeMode mode) => new(
        $"pack://application:,,,/CleanTheme.Wpf;component/Themes/Palette.{(mode == CleanThemeMode.Dark ? "Dark" : "Light")}.xaml",
        UriKind.Absolute);

    void IThemeTarget.Apply(CleanThemeMode actualMode)
    {
        if (!_dispatcher.CheckAccess())
        {
            _dispatcher.BeginInvoke(() => ((IThemeTarget)this).Apply(actualMode));
            return;
        }
        if (actualMode == _applied)
        {
            return;
        }
        _applied = actualMode;
        // Replacing the last merged dictionary invalidates every DynamicResource that reads a palette key.
        MergedDictionaries[^1] = new ResourceDictionary { Source = PaletteUri(actualMode) };
    }
}
