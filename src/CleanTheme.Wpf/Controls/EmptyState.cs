using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CleanTheme.Wpf.Controls;

/// <summary>
/// Centered placeholder for an empty list or view: optional <see cref="Icon"/>, a <see cref="Title"/>, a
/// <see cref="Description"/>, and the content below as the action (e.g. a button).
/// </summary>
public sealed class EmptyState : ContentControl
{
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title), typeof(string), typeof(EmptyState), new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
        nameof(Description), typeof(string), typeof(EmptyState), new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon), typeof(Geometry), typeof(EmptyState), new FrameworkPropertyMetadata(null));

    static EmptyState()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(EmptyState), new FrameworkPropertyMetadata(typeof(EmptyState)));
    }

    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Description
    {
        get => (string?)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>Outline icon drawn above the title (stroked, 32 px box), e.g. a path geometry.</summary>
    public Geometry? Icon
    {
        get => (Geometry?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
}
