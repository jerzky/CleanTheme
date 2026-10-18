using System.Windows;
using System.Windows.Controls;

namespace CleanTheme.Wpf.Controls;

/// <summary>
/// White surface with a title row: <see cref="HeaderedContentControl.Header"/> on the left, <see cref="Actions"/> on the right.
/// The title row collapses when both are null.
/// </summary>
public sealed class Card : HeaderedContentControl
{
    public static readonly DependencyProperty ActionsProperty = DependencyProperty.Register(
        nameof(Actions), typeof(object), typeof(Card), new FrameworkPropertyMetadata(null));

    static Card()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Card), new FrameworkPropertyMetadata(typeof(Card)));
    }

    /// <summary>Content on the right of the title row, typically ghost buttons or a filter.</summary>
    public object? Actions
    {
        get => GetValue(ActionsProperty);
        set => SetValue(ActionsProperty, value);
    }
}
