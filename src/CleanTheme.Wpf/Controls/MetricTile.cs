using System.Windows;
using System.Windows.Controls;

namespace CleanTheme.Wpf.Controls;

/// <summary>A key figure: small <see cref="Label"/> above a large <see cref="Value"/> with an optional <see cref="Unit"/>.</summary>
public sealed class MetricTile : Control
{
    public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
        nameof(Label), typeof(string), typeof(MetricTile), new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value), typeof(object), typeof(MetricTile), new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty UnitProperty = DependencyProperty.Register(
        nameof(Unit), typeof(string), typeof(MetricTile), new FrameworkPropertyMetadata(null));

    static MetricTile()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(MetricTile), new FrameworkPropertyMetadata(typeof(MetricTile)));
    }

    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public object? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string? Unit
    {
        get => (string?)GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }
}
