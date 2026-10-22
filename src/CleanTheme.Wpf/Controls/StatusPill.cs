using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CleanTheme.Wpf.Controls;

/// <summary>Small rounded status label whose colors follow <see cref="State"/>, with optional indicator dot, icon, and dismiss button.</summary>
[TemplatePart(Name = PartCloseButton, Type = typeof(Button))]
public sealed class StatusPill : ContentControl
{
    public const string PartCloseButton = "PART_CloseButton";

    public static readonly DependencyProperty StateProperty = DependencyProperty.Register(
        nameof(State), typeof(StatusState), typeof(StatusPill), new FrameworkPropertyMetadata(StatusState.Neutral));

    public static readonly DependencyProperty ShowDotProperty = DependencyProperty.Register(
        nameof(ShowDot), typeof(bool), typeof(StatusPill), new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon), typeof(Geometry), typeof(StatusPill),
        new FrameworkPropertyMetadata(null, (d, e) => ((StatusPill)d).SetValue(HasIconPropertyKey, e.NewValue != null)));

    private static readonly DependencyPropertyKey HasIconPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(HasIcon), typeof(bool), typeof(StatusPill), new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty HasIconProperty = HasIconPropertyKey.DependencyProperty;

    public bool HasIcon => (bool)GetValue(HasIconProperty);

    public static readonly DependencyProperty IsClosableProperty = DependencyProperty.Register(
        nameof(IsClosable), typeof(bool), typeof(StatusPill), new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty CloseCommandProperty = DependencyProperty.Register(
        nameof(CloseCommand), typeof(ICommand), typeof(StatusPill), new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty CloseCommandParameterProperty = DependencyProperty.Register(
        nameof(CloseCommandParameter), typeof(object), typeof(StatusPill), new FrameworkPropertyMetadata(null));

    public static readonly RoutedEvent ClosedEvent = EventManager.RegisterRoutedEvent(
        nameof(Closed), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(StatusPill));

    static StatusPill()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StatusPill), new FrameworkPropertyMetadata(typeof(StatusPill)));
    }

    public StatusState State
    {
        get => (StatusState)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public bool ShowDot
    {
        get => (bool)GetValue(ShowDotProperty);
        set => SetValue(ShowDotProperty, value);
    }

    public Geometry? Icon
    {
        get => (Geometry?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public bool IsClosable
    {
        get => (bool)GetValue(IsClosableProperty);
        set => SetValue(IsClosableProperty, value);
    }

    public ICommand? CloseCommand
    {
        get => (ICommand?)GetValue(CloseCommandProperty);
        set => SetValue(CloseCommandProperty, value);
    }

    public object? CloseCommandParameter
    {
        get => GetValue(CloseCommandParameterProperty);
        set => SetValue(CloseCommandParameterProperty, value);
    }

    public event RoutedEventHandler Closed
    {
        add => AddHandler(ClosedEvent, value);
        remove => RemoveHandler(ClosedEvent, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        if (GetTemplateChild(PartCloseButton) is Button closeBtn)
        {
            closeBtn.Click -= OnCloseButtonClick;
            closeBtn.Click += OnCloseButtonClick;
        }
    }

    private void OnCloseButtonClick(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        if (CloseCommand != null && CloseCommand.CanExecute(CloseCommandParameter))
        {
            CloseCommand.Execute(CloseCommandParameter);
        }
        RaiseEvent(new RoutedEventArgs(ClosedEvent, this));
    }
}
