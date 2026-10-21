using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace CleanTheme.Wpf.Controls;

/// <summary>
/// A collapsible hamburger navigation menu featuring an icon rail, expandable drawer,
/// header/logo area, footer (e.g. for profile / drop-up menu), and content host.
/// </summary>
public class HamburgerMenu : ListBox
{
    public static readonly DependencyProperty IsPaneOpenProperty = DependencyProperty.Register(
        nameof(IsPaneOpen), typeof(bool), typeof(HamburgerMenu),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsPaneOpenChanged));

    public static readonly DependencyProperty CompactPaneWidthProperty = DependencyProperty.Register(
        nameof(CompactPaneWidth), typeof(double), typeof(HamburgerMenu),
        new FrameworkPropertyMetadata(48.0));

    public static readonly DependencyProperty OpenPaneWidthProperty = DependencyProperty.Register(
        nameof(OpenPaneWidth), typeof(double), typeof(HamburgerMenu),
        new FrameworkPropertyMetadata(240.0));

    public static readonly DependencyProperty PaneBackgroundProperty = DependencyProperty.Register(
        nameof(PaneBackground), typeof(Brush), typeof(HamburgerMenu),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
        nameof(Header), typeof(object), typeof(HamburgerMenu),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.Register(
        nameof(HeaderTemplate), typeof(DataTemplate), typeof(HamburgerMenu),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty CompactHeaderProperty = DependencyProperty.Register(
        nameof(CompactHeader), typeof(object), typeof(HamburgerMenu),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty CompactHeaderTemplateProperty = DependencyProperty.Register(
        nameof(CompactHeaderTemplate), typeof(DataTemplate), typeof(HamburgerMenu),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty FooterProperty = DependencyProperty.Register(
        nameof(Footer), typeof(object), typeof(HamburgerMenu),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty FooterTemplateProperty = DependencyProperty.Register(
        nameof(FooterTemplate), typeof(DataTemplate), typeof(HamburgerMenu),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
        nameof(Content), typeof(object), typeof(HamburgerMenu),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty ContentTemplateProperty = DependencyProperty.Register(
        nameof(ContentTemplate), typeof(DataTemplate), typeof(HamburgerMenu),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty IsHamburgerVisibleProperty = DependencyProperty.Register(
        nameof(IsHamburgerVisible), typeof(bool), typeof(HamburgerMenu),
        new FrameworkPropertyMetadata(true));

    public static readonly DependencyProperty HamburgerToolTipProperty = DependencyProperty.Register(
        nameof(HamburgerToolTip), typeof(string), typeof(HamburgerMenu),
        new FrameworkPropertyMetadata("Toggle navigation"));

    public static readonly RoutedEvent PaneOpenedEvent = EventManager.RegisterRoutedEvent(
        nameof(PaneOpened), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(HamburgerMenu));

    public static readonly RoutedEvent PaneClosedEvent = EventManager.RegisterRoutedEvent(
        nameof(PaneClosed), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(HamburgerMenu));

    static HamburgerMenu()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(HamburgerMenu), new FrameworkPropertyMetadata(typeof(HamburgerMenu)));
        SelectionModeProperty.OverrideMetadata(typeof(HamburgerMenu), new FrameworkPropertyMetadata(SelectionMode.Single));
    }

    /// <summary>Raised when the pane expands.</summary>
    public event RoutedEventHandler PaneOpened
    {
        add => AddHandler(PaneOpenedEvent, value);
        remove => RemoveHandler(PaneOpenedEvent, value);
    }

    /// <summary>Raised when the pane collapses.</summary>
    public event RoutedEventHandler PaneClosed
    {
        add => AddHandler(PaneClosedEvent, value);
        remove => RemoveHandler(PaneClosedEvent, value);
    }

    /// <summary>Whether the navigation drawer is currently open/expanded.</summary>
    public bool IsPaneOpen
    {
        get => (bool)GetValue(IsPaneOpenProperty);
        set => SetValue(IsPaneOpenProperty, value);
    }

    /// <summary>Width of the menu pane when collapsed into an icon rail (default 48 px).</summary>
    public double CompactPaneWidth
    {
        get => (double)GetValue(CompactPaneWidthProperty);
        set => SetValue(CompactPaneWidthProperty, value);
    }

    /// <summary>Width of the menu pane when expanded (default 240 px).</summary>
    public double OpenPaneWidth
    {
        get => (double)GetValue(OpenPaneWidthProperty);
        set => SetValue(OpenPaneWidthProperty, value);
    }

    /// <summary>Background brush for the navigation pane (defaults to Surface).</summary>
    public Brush? PaneBackground
    {
        get => (Brush?)GetValue(PaneBackgroundProperty);
        set => SetValue(PaneBackgroundProperty, value);
    }

    /// <summary>Content shown at the top of the expanded drawer (e.g. large logo, app title).</summary>
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>Template for <see cref="Header"/>.</summary>
    public DataTemplate? HeaderTemplate
    {
        get => (DataTemplate?)GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    /// <summary>Optional compact content shown at the top when the drawer is collapsed (e.g. small logo mark).</summary>
    public object? CompactHeader
    {
        get => GetValue(CompactHeaderProperty);
        set => SetValue(CompactHeaderProperty, value);
    }

    /// <summary>Template for <see cref="CompactHeader"/>.</summary>
    public DataTemplate? CompactHeaderTemplate
    {
        get => (DataTemplate?)GetValue(CompactHeaderTemplateProperty);
        set => SetValue(CompactHeaderTemplateProperty, value);
    }

    /// <summary>Content docked at the bottom of the navigation pane (e.g. a <see cref="HamburgerProfileButton"/> or settings).</summary>
    public object? Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    /// <summary>Template for <see cref="Footer"/>.</summary>
    public DataTemplate? FooterTemplate
    {
        get => (DataTemplate?)GetValue(FooterTemplateProperty);
        set => SetValue(FooterTemplateProperty, value);
    }

    /// <summary>Main view content hosted alongside the navigation menu.</summary>
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    /// <summary>Template for <see cref="Content"/>.</summary>
    public DataTemplate? ContentTemplate
    {
        get => (DataTemplate?)GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }

    /// <summary>Whether the hamburger toggle button is visible.</summary>
    public bool IsHamburgerVisible
    {
        get => (bool)GetValue(IsHamburgerVisibleProperty);
        set => SetValue(IsHamburgerVisibleProperty, value);
    }

    /// <summary>Tooltip for the hamburger toggle button.</summary>
    public string? HamburgerToolTip
    {
        get => (string?)GetValue(HamburgerToolTipProperty);
        set => SetValue(HamburgerToolTipProperty, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (GetTemplateChild("PART_HamburgerButton") is ButtonBase hamburgerButton)
        {
            hamburgerButton.Click -= OnHamburgerButtonClick;
            hamburgerButton.Click += OnHamburgerButtonClick;
        }

        if (GetTemplateChild("PART_CompactHamburgerButton") is ButtonBase compactHamburgerButton)
        {
            compactHamburgerButton.Click -= OnHamburgerButtonClick;
            compactHamburgerButton.Click += OnHamburgerButtonClick;
        }
    }

    private void OnHamburgerButtonClick(object sender, RoutedEventArgs e)
    {
        SetCurrentValue(IsPaneOpenProperty, !IsPaneOpen);
    }

    private static void OnIsPaneOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HamburgerMenu menu)
        {
            bool isOpen = (bool)e.NewValue;
            menu.RaiseEvent(new RoutedEventArgs(isOpen ? PaneOpenedEvent : PaneClosedEvent, menu));
        }
    }

    protected override DependencyObject GetContainerForItemOverride() => new HamburgerMenuItem();

    protected override bool IsItemItsOwnContainerOverride(object item) => item is HamburgerMenuItem;
}
