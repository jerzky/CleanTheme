using System.Windows;
using System.Windows.Controls.Primitives;

namespace CleanTheme.Wpf.Controls;

/// <summary>Which button closed a <see cref="CleanDialog"/>.</summary>
public enum CleanDialogResult
{
    /// <summary>Closed another way (title bar, Alt+F4) or not closed yet.</summary>
    None,
    Primary,
    Secondary,
}

/// <summary>
/// A themed dialog window: <see cref="Heading"/>, the content as body, and a footer with a primary and an optional
/// secondary button. <see cref="ShowModal"/> returns which one was pressed. Enter presses primary, Escape secondary.
/// </summary>
[TemplatePart(Name = PrimaryButtonPart, Type = typeof(ButtonBase))]
[TemplatePart(Name = SecondaryButtonPart, Type = typeof(ButtonBase))]
public class CleanDialog : Window
{
    private const string PrimaryButtonPart = "PART_PrimaryButton";
    private const string SecondaryButtonPart = "PART_SecondaryButton";

    public static readonly DependencyProperty HeadingProperty = DependencyProperty.Register(
        nameof(Heading), typeof(string), typeof(CleanDialog), new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty PrimaryButtonTextProperty = DependencyProperty.Register(
        nameof(PrimaryButtonText), typeof(string), typeof(CleanDialog), new FrameworkPropertyMetadata("OK"));

    public static readonly DependencyProperty SecondaryButtonTextProperty = DependencyProperty.Register(
        nameof(SecondaryButtonText), typeof(string), typeof(CleanDialog), new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty IsDangerProperty = DependencyProperty.Register(
        nameof(IsDanger), typeof(bool), typeof(CleanDialog), new FrameworkPropertyMetadata(false));

    private ButtonBase? _primary;
    private ButtonBase? _secondary;

    static CleanDialog()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CleanDialog), new FrameworkPropertyMetadata(typeof(CleanDialog)));
    }

    public CleanDialog()
    {
        // Not a dependency property, so the style cannot set it.
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
    }

    /// <summary>Bold first line of the body.</summary>
    public string? Heading
    {
        get => (string?)GetValue(HeadingProperty);
        set => SetValue(HeadingProperty, value);
    }

    /// <summary>Text of the default (Enter) button. "OK" by default.</summary>
    public string? PrimaryButtonText
    {
        get => (string?)GetValue(PrimaryButtonTextProperty);
        set => SetValue(PrimaryButtonTextProperty, value);
    }

    /// <summary>Text of the cancel (Escape) button; null hides it.</summary>
    public string? SecondaryButtonText
    {
        get => (string?)GetValue(SecondaryButtonTextProperty);
        set => SetValue(SecondaryButtonTextProperty, value);
    }

    /// <summary>Shows the primary button as destructive (red).</summary>
    public bool IsDanger
    {
        get => (bool)GetValue(IsDangerProperty);
        set => SetValue(IsDangerProperty, value);
    }

    /// <summary>The button that closed the dialog.</summary>
    public CleanDialogResult Result { get; private set; }

    /// <summary>Shows the dialog modally and returns the button that closed it.</summary>
    public CleanDialogResult ShowModal()
    {
        Result = CleanDialogResult.None;
        ShowDialog();
        return Result;
    }

    public override void OnApplyTemplate()
    {
        if (_primary != null)
        {
            _primary.Click -= OnPrimaryClick;
        }
        if (_secondary != null)
        {
            _secondary.Click -= OnSecondaryClick;
        }
        base.OnApplyTemplate();
        _primary = GetTemplateChild(PrimaryButtonPart) as ButtonBase;
        _secondary = GetTemplateChild(SecondaryButtonPart) as ButtonBase;
        if (_primary != null)
        {
            _primary.Click += OnPrimaryClick;
        }
        if (_secondary != null)
        {
            _secondary.Click += OnSecondaryClick;
        }
    }

    /// <summary>Called before the dialog closes from the primary button; return false to keep it open (e.g. invalid input).</summary>
    protected virtual bool OnPrimary() => true;

    private void OnPrimaryClick(object sender, RoutedEventArgs e)
    {
        if (OnPrimary())
        {
            Result = CleanDialogResult.Primary;
            Close();
        }
    }

    private void OnSecondaryClick(object sender, RoutedEventArgs e)
    {
        Result = CleanDialogResult.Secondary;
        Close();
    }
}
