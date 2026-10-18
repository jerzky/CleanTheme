using System.Windows;
using System.Windows.Controls.Primitives;

namespace CleanTheme.Wpf.Controls;

/// <summary>An on/off switch with its label (the content) to the right. Bind <c>IsChecked</c>.</summary>
public sealed class ToggleSwitch : ToggleButton
{
    static ToggleSwitch()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ToggleSwitch), new FrameworkPropertyMetadata(typeof(ToggleSwitch)));
        IsThreeStateProperty.OverrideMetadata(typeof(ToggleSwitch), new FrameworkPropertyMetadata(false));
    }
}
