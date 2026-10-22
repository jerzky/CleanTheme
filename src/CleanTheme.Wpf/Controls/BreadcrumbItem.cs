using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CleanTheme.Wpf.Controls;

/// <summary>An individual navigation step in a <see cref="BreadcrumbBar"/>.</summary>
public class BreadcrumbItem : HeaderedContentControl, ICommandSource
{
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon), typeof(Geometry), typeof(BreadcrumbItem),
        new FrameworkPropertyMetadata(null, (d, e) => ((BreadcrumbItem)d).SetValue(HasIconPropertyKey, e.NewValue != null)));

    private static readonly DependencyPropertyKey HasIconPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(HasIcon), typeof(bool), typeof(BreadcrumbItem), new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty HasIconProperty = HasIconPropertyKey.DependencyProperty;

    public bool HasIcon => (bool)GetValue(HasIconProperty);

    public static readonly DependencyProperty IsCurrentProperty = DependencyProperty.Register(
        nameof(IsCurrent), typeof(bool), typeof(BreadcrumbItem), new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
        nameof(Command), typeof(ICommand), typeof(BreadcrumbItem), new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
        nameof(CommandParameter), typeof(object), typeof(BreadcrumbItem), new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register(
        nameof(CommandTarget), typeof(IInputElement), typeof(BreadcrumbItem), new FrameworkPropertyMetadata(null));

    static BreadcrumbItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(BreadcrumbItem), new FrameworkPropertyMetadata(typeof(BreadcrumbItem)));
    }

    public Geometry? Icon
    {
        get => (Geometry?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public bool IsCurrent
    {
        get => (bool)GetValue(IsCurrentProperty);
        set => SetValue(IsCurrentProperty, value);
    }

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public IInputElement? CommandTarget
    {
        get => (IInputElement?)GetValue(CommandTargetProperty);
        set => SetValue(CommandTargetProperty, value);
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);
        if (!IsCurrent && Command != null && Command.CanExecute(CommandParameter))
        {
            Command.Execute(CommandParameter);
            e.Handled = true;
        }
    }
}
