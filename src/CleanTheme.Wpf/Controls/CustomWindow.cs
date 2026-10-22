using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shell;

namespace CleanTheme.Wpf.Controls;

/// <summary>
/// A modern chromeless window with a customizable title bar, window control buttons (minimize, maximize/restore, close),
/// native dragging/snapping gestures, and automatic theme adaptation.
/// </summary>
[TemplatePart(Name = PartTitleBar, Type = typeof(FrameworkElement))]
[TemplatePart(Name = PartMinimizeButton, Type = typeof(Button))]
[TemplatePart(Name = PartMaximizeButton, Type = typeof(Button))]
[TemplatePart(Name = PartCloseButton, Type = typeof(Button))]
public class CustomWindow : Window
{
    public const string PartTitleBar = "PART_TitleBar";
    public const string PartMinimizeButton = "PART_MinimizeButton";
    public const string PartMaximizeButton = "PART_MaximizeButton";
    public const string PartCloseButton = "PART_CloseButton";

    public static readonly DependencyProperty TitleBarContentProperty = DependencyProperty.Register(
        nameof(TitleBarContent), typeof(object), typeof(CustomWindow), new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty TitleBarHeightProperty = DependencyProperty.Register(
        nameof(TitleBarHeight), typeof(double), typeof(CustomWindow),
        new FrameworkPropertyMetadata(36.0, OnTitleBarMetricsChanged));

    public static readonly DependencyProperty IsTitleBarVisibleProperty = DependencyProperty.Register(
        nameof(IsTitleBarVisible), typeof(bool), typeof(CustomWindow),
        new FrameworkPropertyMetadata(true, OnTitleBarMetricsChanged));

    public static readonly DependencyProperty ShowMinimizeButtonProperty = DependencyProperty.Register(
        nameof(ShowMinimizeButton), typeof(bool), typeof(CustomWindow), new FrameworkPropertyMetadata(true));

    public static readonly DependencyProperty ShowMaximizeButtonProperty = DependencyProperty.Register(
        nameof(ShowMaximizeButton), typeof(bool), typeof(CustomWindow), new FrameworkPropertyMetadata(true));

    public static readonly DependencyProperty ShowCloseButtonProperty = DependencyProperty.Register(
        nameof(ShowCloseButton), typeof(bool), typeof(CustomWindow), new FrameworkPropertyMetadata(true));

    private static readonly DependencyPropertyKey HasIconPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(HasIcon), typeof(bool), typeof(CustomWindow), new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty HasIconProperty = HasIconPropertyKey.DependencyProperty;

    public bool HasIcon => (bool)GetValue(HasIconProperty);

    private WindowChrome? _chrome;
    private FrameworkElement? _titleBar;
    private Button? _minimizeButton;
    private Button? _maximizeButton;
    private Button? _closeButton;

    static CustomWindow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomWindow), new FrameworkPropertyMetadata(typeof(CustomWindow)));
    }

    public CustomWindow()
    {
        WindowTheme.SetFollowTheme(this, true);
        SetupWindowChrome();
        CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, (_, _) => SystemCommands.CloseWindow(this)));
        CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, (_, _) => SystemCommands.MinimizeWindow(this)));
        CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, (_, _) => SystemCommands.MaximizeWindow(this)));
        CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, (_, _) => SystemCommands.RestoreWindow(this)));
    }

    public object? TitleBarContent
    {
        get => GetValue(TitleBarContentProperty);
        set => SetValue(TitleBarContentProperty, value);
    }

    public double TitleBarHeight
    {
        get => (double)GetValue(TitleBarHeightProperty);
        set => SetValue(TitleBarHeightProperty, value);
    }

    public bool IsTitleBarVisible
    {
        get => (bool)GetValue(IsTitleBarVisibleProperty);
        set => SetValue(IsTitleBarVisibleProperty, value);
    }

    public bool ShowMinimizeButton
    {
        get => (bool)GetValue(ShowMinimizeButtonProperty);
        set => SetValue(ShowMinimizeButtonProperty, value);
    }

    public bool ShowMaximizeButton
    {
        get => (bool)GetValue(ShowMaximizeButtonProperty);
        set => SetValue(ShowMaximizeButtonProperty, value);
    }

    public bool ShowCloseButton
    {
        get => (bool)GetValue(ShowCloseButtonProperty);
        set => SetValue(ShowCloseButtonProperty, value);
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.Property == IconProperty)
        {
            SetValue(HasIconPropertyKey, Icon != null);
        }
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_titleBar != null)
        {
            _titleBar.MouseLeftButtonDown -= OnTitleBarMouseLeftButtonDown;
            _titleBar.MouseRightButtonUp -= OnTitleBarMouseRightButtonUp;
        }

        if (_minimizeButton != null) _minimizeButton.Click -= OnMinimizeClick;
        if (_maximizeButton != null) _maximizeButton.Click -= OnMaximizeClick;
        if (_closeButton != null) _closeButton.Click -= OnCloseClick;

        _titleBar = GetTemplateChild(PartTitleBar) as FrameworkElement;
        _minimizeButton = GetTemplateChild(PartMinimizeButton) as Button;
        _maximizeButton = GetTemplateChild(PartMaximizeButton) as Button;
        _closeButton = GetTemplateChild(PartCloseButton) as Button;

        if (_titleBar != null)
        {
            _titleBar.MouseLeftButtonDown += OnTitleBarMouseLeftButtonDown;
            _titleBar.MouseRightButtonUp += OnTitleBarMouseRightButtonUp;
        }

        if (_minimizeButton != null) _minimizeButton.Click += OnMinimizeClick;
        if (_maximizeButton != null) _maximizeButton.Click += OnMaximizeClick;
        if (_closeButton != null) _closeButton.Click += OnCloseClick;
    }

    private void SetupWindowChrome()
    {
        _chrome = new WindowChrome
        {
            CaptionHeight = IsTitleBarVisible ? TitleBarHeight : 0,
            ResizeBorderThickness = new Thickness(6),
            CornerRadius = new CornerRadius(0),
            GlassFrameThickness = new Thickness(0),
            UseAeroCaptionButtons = false,
        };
        WindowChrome.SetWindowChrome(this, _chrome);
    }

    private static void OnTitleBarMetricsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CustomWindow w && w._chrome != null)
        {
            w._chrome.CaptionHeight = w.IsTitleBarVisible ? w.TitleBarHeight : 0;
        }
    }

    private void OnTitleBarMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2 && (ResizeMode is ResizeMode.CanResize or ResizeMode.CanResizeWithGrip))
        {
            ToggleMaximizeRestore();
        }
    }

    private void OnTitleBarMouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        Point screenPoint = PointToScreen(e.GetPosition(this));
        SystemCommands.ShowSystemMenu(this, screenPoint);
    }

    private void OnMinimizeClick(object sender, RoutedEventArgs e)
    {
        SystemCommands.MinimizeWindow(this);
    }

    private void OnMaximizeClick(object sender, RoutedEventArgs e)
    {
        ToggleMaximizeRestore();
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        SystemCommands.CloseWindow(this);
    }

    private void ToggleMaximizeRestore()
    {
        if (WindowState == WindowState.Maximized)
        {
            SystemCommands.RestoreWindow(this);
        }
        else
        {
            SystemCommands.MaximizeWindow(this);
        }
    }
}
