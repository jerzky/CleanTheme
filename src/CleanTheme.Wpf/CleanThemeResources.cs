using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace CleanTheme.Wpf;

/// <summary>
/// The CleanTheme resource dictionary, for merging into application or window resources:
/// <c>&lt;ResourceDictionary.MergedDictionaries&gt;&lt;clean:CleanThemeResources /&gt;&lt;/ResourceDictionary.MergedDictionaries&gt;</c>.
/// </summary>
[SuppressMessage("Design", "CA1010:Generic interface should also be implemented", Justification = "The collection surface is WPF's ResourceDictionary; this type only sets Source.")]
public sealed class CleanThemeResources : ResourceDictionary
{
    /// <summary>Pack URI of the theme dictionary, for merging by <c>Source</c> instead of by type.</summary>
    public static Uri ThemeUri { get; } = new("pack://application:,,,/CleanTheme.Wpf;component/Themes/CleanTheme.xaml", UriKind.Absolute);

    /// <summary>Loads the theme.</summary>
    public CleanThemeResources() => Source = ThemeUri;
}
