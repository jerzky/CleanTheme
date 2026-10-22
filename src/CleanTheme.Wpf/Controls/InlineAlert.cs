using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CleanTheme.Wpf.Controls;

/// <summary>An inline banner alert with title, message, semantic styling, optional action link, and dismiss button.</summary>
[TemplatePart(Name = PartCloseButton, Type = typeof(Button))]
[TemplatePart(Name = PartActionButton, Type = typeof(Button))]
public class InlineAlert : ContentControl
{
    public const string PartCloseButton = "PART_CloseButton";
    public const string PartActionButton = "PART_ActionButton";

    public static readonly DependencyProperty StateProperty = DependencyProperty.Register(
        nameof(State), typeof(StatusState), typeof(InlineAlert), new FrameworkPropertyMetadata(StatusState.Info));

    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title), typeof(string), typeof(InlineAlert), new FrameworkPropertyMetadata(string.Empty));

    public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
        nameof(Message), typeof(object), typeof(InlineAlert), new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon), typeof(Geometry), typeof(InlineAlert), new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty IsDismissibleProperty = DependencyProperty.Register(
        nameof(IsDismissible), typeof(bool), typeof(InlineAlert), new FrameworkPropertyMetadata(true));

    public static readonly DependencyProperty IsDismissedProperty = DependencyProperty.Register(
        nameof(IsDismissed), typeof(bool), typeof(InlineAlert),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsDismissedChanged));

    public static readonly DependencyProperty DismissCommandProperty = DependencyProperty.Register(
        nameof(DismissCommand), typeof(ICommand), typeof(InlineAlert), new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty ActionTextProperty = DependencyProperty.Register(
        nameof(ActionText), typeof(string), typeof(InlineAlert),
        new FrameworkPropertyMetadata(null, (d, e) => ((InlineAlert)d).SetValue(HasActionTextPropertyKey, !string.IsNullOrEmpty(e.NewValue as string))));

    private static readonly DependencyPropertyKey HasActionTextPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(HasActionText), typeof(bool), typeof(InlineAlert), new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty HasActionTextProperty = HasActionTextPropertyKey.DependencyProperty;

    public bool HasActionText => (bool)GetValue(HasActionTextProperty);

    public static readonly DependencyProperty ActionCommandProperty = DependencyProperty.Register(
        nameof(ActionCommand), typeof(ICommand), typeof(InlineAlert), new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty ActionCommandParameterProperty = DependencyProperty.Register(
        nameof(ActionCommandParameter), typeof(object), typeof(InlineAlert), new FrameworkPropertyMetadata(null));

    public static readonly RoutedEvent DismissedEvent = EventManager.RegisterRoutedEvent(
        nameof(Dismissed), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(InlineAlert));

    static InlineAlert()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(InlineAlert), new FrameworkPropertyMetadata(typeof(InlineAlert)));
    }

    public StatusState State
    {
        get => (StatusState)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public object? Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public Geometry? Icon
    {
        get => (Geometry?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public bool IsDismissible
    {
        get => (bool)GetValue(IsDismissibleProperty);
        set => SetValue(IsDismissibleProperty, value);
    }

    public bool IsDismissed
    {
        get => (bool)GetValue(IsDismissedProperty);
        set => SetValue(IsDismissedProperty, value);
    }

    public ICommand? DismissCommand
    {
        get => (ICommand?)GetValue(DismissCommandProperty);
        set => SetValue(DismissCommandProperty, value);
    }

    public string? ActionText
    {
        get => (string?)GetValue(ActionTextProperty);
        set => SetValue(ActionTextProperty, value);
    }

    public ICommand? ActionCommand
    {
        get => (ICommand?)GetValue(ActionCommandProperty);
        set => SetValue(ActionCommandProperty, value);
    }

    public object? ActionCommandParameter
    {
        get => GetValue(ActionCommandParameterProperty);
        set => SetValue(ActionCommandParameterProperty, value);
    }

    public event RoutedEventHandler Dismissed
    {
        add => AddHandler(DismissedEvent, value);
        remove => RemoveHandler(DismissedEvent, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        if (GetTemplateChild(PartCloseButton) is Button closeBtn)
        {
            closeBtn.Click -= OnCloseButtonClick;
            closeBtn.Click += OnCloseButtonClick;
        }
        if (GetTemplateChild(PartActionButton) is Button actionBtn)
        {
            actionBtn.Click -= OnActionButtonClick;
            actionBtn.Click += OnActionButtonClick;
        }
    }

    private void OnCloseButtonClick(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        Dismiss();
    }

    private void OnActionButtonClick(object sender, RoutedEventArgs e)
    {
        if (ActionCommand != null && ActionCommand.CanExecute(ActionCommandParameter))
        {
            ActionCommand.Execute(ActionCommandParameter);
        }
    }

    public void Dismiss()
    {
        IsDismissed = true;
        if (DismissCommand != null && DismissCommand.CanExecute(null))
        {
            DismissCommand.Execute(null);
        }
        RaiseEvent(new RoutedEventArgs(DismissedEvent, this));
    }

    private static void OnIsDismissedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is InlineAlert alert)
        {
            alert.Visibility = (bool)e.NewValue ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
