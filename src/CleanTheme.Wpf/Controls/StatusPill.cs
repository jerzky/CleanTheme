using System.Windows;
using System.Windows.Controls;

namespace CleanTheme.Wpf.Controls;

/// <summary>Small rounded status label whose colors follow <see cref="State"/>.</summary>
public sealed class StatusPill : ContentControl
{
    public static readonly DependencyProperty StateProperty = DependencyProperty.Register(
        nameof(State), typeof(StatusState), typeof(StatusPill), new FrameworkPropertyMetadata(StatusState.Neutral));

    static StatusPill()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StatusPill), new FrameworkPropertyMetadata(typeof(StatusPill)));
    }

    public StatusState State
    {
        get => (StatusState)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }
}
