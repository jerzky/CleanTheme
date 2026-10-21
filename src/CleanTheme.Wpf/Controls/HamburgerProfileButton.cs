using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace CleanTheme.Wpf.Controls;

/// <summary>
/// A dense user profile button docked in the footer of a <see cref="HamburgerMenu"/>.
/// Displays an avatar, display name, optional subtitle/badge, and opens an attached
/// <see cref="FrameworkElement.ContextMenu"/> on primary click (drop-up).
/// In collapsed mode, it condenses to just the circular avatar badge.
/// </summary>
public class HamburgerProfileButton : ButtonBase
{
    public static readonly DependencyProperty AvatarProperty = DependencyProperty.Register(
        nameof(Avatar), typeof(object), typeof(HamburgerProfileButton),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty AvatarBackgroundProperty = DependencyProperty.Register(
        nameof(AvatarBackground), typeof(Brush), typeof(HamburgerProfileButton),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty AvatarForegroundProperty = DependencyProperty.Register(
        nameof(AvatarForeground), typeof(Brush), typeof(HamburgerProfileButton),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty DisplayNameProperty = DependencyProperty.Register(
        nameof(DisplayName), typeof(string), typeof(HamburgerProfileButton),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty SubtitleProperty = DependencyProperty.Register(
        nameof(Subtitle), typeof(string), typeof(HamburgerProfileButton),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty TrailingIconProperty = DependencyProperty.Register(
        nameof(TrailingIcon), typeof(Geometry), typeof(HamburgerProfileButton),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty TrailingToolTipProperty = DependencyProperty.Register(
        nameof(TrailingToolTip), typeof(string), typeof(HamburgerProfileButton),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty TrailingCommandProperty = DependencyProperty.Register(
        nameof(TrailingCommand), typeof(ICommand), typeof(HamburgerProfileButton),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty OpensMenuOnClickProperty = DependencyProperty.Register(
        nameof(OpensMenuOnClick), typeof(bool), typeof(HamburgerProfileButton),
        new FrameworkPropertyMetadata(true));

    static HamburgerProfileButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(HamburgerProfileButton), new FrameworkPropertyMetadata(typeof(HamburgerProfileButton)));
    }

    public HamburgerProfileButton()
    {
        Click += OnProfileButtonClick;
    }

    private void OnProfileButtonClick(object sender, RoutedEventArgs e)
    {
        if (!OpensMenuOnClick || ContextMenu is null) return;
        if (!ReferenceEquals(e.Source, this)) return;

        ContextMenu.PlacementTarget = this;
        ContextMenu.Placement = PlacementMode.Top;
        ContextMenu.IsOpen = !ContextMenu.IsOpen;
    }

    /// <summary>Avatar content (initials, icon, or image).</summary>
    public object? Avatar
    {
        get => GetValue(AvatarProperty);
        set => SetValue(AvatarProperty, value);
    }

    /// <summary>Background brush for the avatar circle (defaults to AccentSoft).</summary>
    public Brush? AvatarBackground
    {
        get => (Brush?)GetValue(AvatarBackgroundProperty);
        set => SetValue(AvatarBackgroundProperty, value);
    }

    /// <summary>Foreground brush for the avatar circle (defaults to Accent).</summary>
    public Brush? AvatarForeground
    {
        get => (Brush?)GetValue(AvatarForegroundProperty);
        set => SetValue(AvatarForegroundProperty, value);
    }

    /// <summary>Primary username or label (e.g. "jens").</summary>
    public string? DisplayName
    {
        get => (string?)GetValue(DisplayNameProperty);
        set => SetValue(DisplayNameProperty, value);
    }

    /// <summary>Optional subtitle or tier badge (e.g. "Pro" or "Admin").</summary>
    public string? Subtitle
    {
        get => (string?)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    /// <summary>Optional trailing action icon on the right (e.g. bug report / feedback).</summary>
    public Geometry? TrailingIcon
    {
        get => (Geometry?)GetValue(TrailingIconProperty);
        set => SetValue(TrailingIconProperty, value);
    }

    /// <summary>Tooltip for the trailing action icon.</summary>
    public string? TrailingToolTip
    {
        get => (string?)GetValue(TrailingToolTipProperty);
        set => SetValue(TrailingToolTipProperty, value);
    }

    /// <summary>Command for the trailing action icon.</summary>
    public ICommand? TrailingCommand
    {
        get => (ICommand?)GetValue(TrailingCommandProperty);
        set => SetValue(TrailingCommandProperty, value);
    }

    /// <summary>Whether left-clicking opens the attached <see cref="FrameworkElement.ContextMenu"/> upward (default true).</summary>
    public bool OpensMenuOnClick
    {
        get => (bool)GetValue(OpensMenuOnClickProperty);
        set => SetValue(OpensMenuOnClickProperty, value);
    }

    protected override void OnClick()
    {
        base.OnClick();
    }
}
