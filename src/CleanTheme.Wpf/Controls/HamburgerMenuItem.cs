using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CleanTheme.Wpf.Controls;

/// <summary>
/// An item inside a <see cref="HamburgerMenu"/>.
/// Displays an icon, text label, optional badge, and supports MVVM command execution on click.
/// </summary>
public class HamburgerMenuItem : ListBoxItem, ICommandSource
{
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon), typeof(Geometry), typeof(HamburgerMenuItem),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty IconElementProperty = DependencyProperty.Register(
        nameof(IconElement), typeof(object), typeof(HamburgerMenuItem),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text), typeof(string), typeof(HamburgerMenuItem),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty BadgeProperty = DependencyProperty.Register(
        nameof(Badge), typeof(object), typeof(HamburgerMenuItem),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty IsPaneOpenProperty = DependencyProperty.Register(
        nameof(IsPaneOpen), typeof(bool), typeof(HamburgerMenuItem),
        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
        nameof(Command), typeof(ICommand), typeof(HamburgerMenuItem),
        new PropertyMetadata(null, OnCommandChanged));

    public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
        nameof(CommandParameter), typeof(object), typeof(HamburgerMenuItem),
        new PropertyMetadata(null));

    public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register(
        nameof(CommandTarget), typeof(IInputElement), typeof(HamburgerMenuItem),
        new PropertyMetadata(null));

    static HamburgerMenuItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(HamburgerMenuItem), new FrameworkPropertyMetadata(typeof(HamburgerMenuItem)));
    }

    /// <summary>Vector outline geometry for the item's icon (rendered with 16x16 box).</summary>
    public Geometry? Icon
    {
        get => (Geometry?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Custom visual element for the icon when not using vector <see cref="Icon"/>.</summary>
    public object? IconElement
    {
        get => GetValue(IconElementProperty);
        set => SetValue(IconElementProperty, value);
    }

    /// <summary>Text label displayed when the drawer is expanded.</summary>
    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>Optional count or badge label (e.g. 14, "New") displayed next to the text.</summary>
    public object? Badge
    {
        get => GetValue(BadgeProperty);
        set => SetValue(BadgeProperty, value);
    }

    /// <summary>Whether the hosting navigation drawer is currently open/expanded.</summary>
    public bool IsPaneOpen
    {
        get => (bool)GetValue(IsPaneOpenProperty);
        set => SetValue(IsPaneOpenProperty, value);
    }

    /// <summary>Command executed when this item is selected.</summary>
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>Parameter passed to <see cref="Command"/>.</summary>
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    /// <summary>Target element on which the routed command is raised.</summary>
    public IInputElement? CommandTarget
    {
        get => (IInputElement?)GetValue(CommandTargetProperty);
        set => SetValue(CommandTargetProperty, value);
    }

    protected override void OnSelected(RoutedEventArgs e)
    {
        base.OnSelected(e);
        ExecuteCommand();
    }

    private void ExecuteCommand()
    {
        if (Command is null) return;
        var param = CommandParameter;
        if (Command is RoutedCommand routed)
        {
            if (routed.CanExecute(param, CommandTarget ?? this))
            {
                routed.Execute(param, CommandTarget ?? this);
            }
        }
        else if (Command.CanExecute(param))
        {
            Command.Execute(param);
        }
    }

    private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HamburgerMenuItem item)
        {
            if (e.OldValue is ICommand oldCommand)
            {
                oldCommand.CanExecuteChanged -= item.OnCanExecuteChanged;
            }
            if (e.NewValue is ICommand newCommand)
            {
                newCommand.CanExecuteChanged += item.OnCanExecuteChanged;
                item.UpdateCanExecute();
            }
        }
    }

    private void OnCanExecuteChanged(object? sender, EventArgs e) => UpdateCanExecute();

    private void UpdateCanExecute()
    {
        if (Command is not null)
        {
            var param = CommandParameter;
            bool canExecute = Command is RoutedCommand routed
                ? routed.CanExecute(param, CommandTarget ?? this)
                : Command.CanExecute(param);
            IsEnabled = canExecute;
        }
    }
}
