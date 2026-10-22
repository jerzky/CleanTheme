using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CleanTheme.Wpf.Controls;

/// <summary>Circular user or entity avatar displaying an image or fallback initials, with optional status badge.</summary>
public class Avatar : Control
{
    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
        nameof(Source), typeof(ImageSource), typeof(Avatar),
        new FrameworkPropertyMetadata(null, (d, e) => ((Avatar)d).SetValue(HasImagePropertyKey, e.NewValue != null)));

    private static readonly DependencyPropertyKey HasImagePropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(HasImage), typeof(bool), typeof(Avatar), new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty HasImageProperty = HasImagePropertyKey.DependencyProperty;

    public bool HasImage => (bool)GetValue(HasImageProperty);

    public static readonly DependencyProperty InitialsProperty = DependencyProperty.Register(
        nameof(Initials), typeof(string), typeof(Avatar), new FrameworkPropertyMetadata(string.Empty));

    public static readonly DependencyProperty AvatarSizeProperty = DependencyProperty.Register(
        nameof(AvatarSize), typeof(double), typeof(Avatar), new FrameworkPropertyMetadata(36.0));

    public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(
        nameof(Status), typeof(AvatarStatus), typeof(Avatar), new FrameworkPropertyMetadata(AvatarStatus.None));

    public static readonly DependencyProperty BadgeProperty = DependencyProperty.Register(
        nameof(Badge), typeof(object), typeof(Avatar), new FrameworkPropertyMetadata(null));

    static Avatar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Avatar), new FrameworkPropertyMetadata(typeof(Avatar)));
    }

    public ImageSource? Source
    {
        get => (ImageSource?)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public string Initials
    {
        get => (string)GetValue(InitialsProperty);
        set => SetValue(InitialsProperty, value);
    }

    public double AvatarSize
    {
        get => (double)GetValue(AvatarSizeProperty);
        set => SetValue(AvatarSizeProperty, value);
    }

    public AvatarStatus Status
    {
        get => (AvatarStatus)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    public object? Badge
    {
        get => GetValue(BadgeProperty);
        set => SetValue(BadgeProperty, value);
    }
}
