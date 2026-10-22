using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CleanTheme.Wpf.Controls;

namespace CleanTheme.Wpf.Tests;

public sealed class UntitledUiControlsTests
{
    private sealed class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        public RelayCommand(Action<object?> execute) => _execute = execute;
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => _execute(parameter);
        public event EventHandler? CanExecuteChanged { add { } remove { } }
    }

    [Fact]
    public void StatusPill_supports_dot_icon_and_dismiss() => Sta.Run(() =>
    {
        bool commandExecuted = false;
        bool eventFired = false;

        var pill = new StatusPill
        {
            Content = "Active",
            State = StatusState.Positive,
            ShowDot = true,
            IsClosable = true,
            CloseCommand = new RelayCommand(_ => commandExecuted = true)
        };
        pill.Closed += (_, _) => eventFired = true;

        Sta.Layout(pill);

        Assert.True(pill.ShowDot);
        Assert.True(pill.IsClosable);

        var dot = (FrameworkElement)pill.Template.FindName("Dot", pill);
        Assert.NotNull(dot);
        Assert.Equal(Visibility.Visible, dot.Visibility);

        var closeBtn = (Button)pill.Template.FindName(StatusPill.PartCloseButton, pill);
        Assert.NotNull(closeBtn);
        Assert.Equal(Visibility.Visible, closeBtn.Visibility);

        // Click close
        closeBtn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Assert.True(commandExecuted);
        Assert.True(eventFired);
    });

    [Fact]
    public void Avatar_renders_initials_and_status() => Sta.Run(() =>
    {
        var avatar = new Avatar
        {
            Initials = "JD",
            AvatarSize = 40,
            Status = AvatarStatus.Online
        };

        Sta.Layout(avatar);

        Assert.Equal("JD", avatar.Initials);
        Assert.Equal(40.0, avatar.AvatarSize);
        Assert.Equal(AvatarStatus.Online, avatar.Status);

        var statusInd = (FrameworkElement)avatar.Template.FindName("StatusIndicator", avatar);
        Assert.NotNull(statusInd);
        Assert.Equal(Visibility.Visible, statusInd.Visibility);
    });

    [Fact]
    public void AvatarGroup_calculates_visible_and_overflow_count() => Sta.Run(() =>
    {
        var group = new AvatarGroup { MaxDisplay = 3 };
        group.ItemsSource = new List<Avatar>
        {
            new() { Initials = "A" },
            new() { Initials = "B" },
            new() { Initials = "C" },
            new() { Initials = "D" },
            new() { Initials = "E" }
        };

        Sta.Layout(group);
        Assert.Equal(2, group.OverflowCount);
        Assert.True(group.HasOverflow);

        Sta.Layout(group);
        var overflowPill = (FrameworkElement)group.Template.FindName("OverflowPill", group);
        Assert.NotNull(overflowPill);
        Assert.Equal(Visibility.Visible, overflowPill.Visibility);
    });

    [Fact]
    public void InlineAlert_state_and_dismiss_behavior() => Sta.Run(() =>
    {
        bool dismissed = false;
        var alert = new InlineAlert
        {
            Title = "Update available",
            Message = "A new version is ready to install.",
            State = StatusState.Info,
            IsDismissible = true,
            DismissCommand = new RelayCommand(_ => dismissed = true)
        };

        Sta.Layout(alert);
        Assert.False(alert.IsDismissed);
        Assert.Equal(Visibility.Visible, alert.Visibility);

        var closeBtn = (Button)alert.Template.FindName(InlineAlert.PartCloseButton, alert);
        Assert.NotNull(closeBtn);
        Assert.Equal(Visibility.Visible, closeBtn.Visibility);

        closeBtn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Assert.True(alert.IsDismissed);
        Assert.True(dismissed);
        Assert.Equal(Visibility.Collapsed, alert.Visibility);
    });

    [Fact]
    public void BreadcrumbBar_sets_current_page_on_last_item() => Sta.Run(() =>
    {
        var bar = new BreadcrumbBar();
        var item1 = new BreadcrumbItem { Header = "Home" };
        var item2 = new BreadcrumbItem { Header = "Settings" };
        var item3 = new BreadcrumbItem { Header = "Team" };

        bar.Items.Add(item1);
        bar.Items.Add(item2);
        bar.Items.Add(item3);

        Sta.Layout(bar);

        Assert.False(item1.IsCurrent);
        Assert.False(item2.IsCurrent);
        Assert.True(item3.IsCurrent);
    });

    [Fact]
    public void PaginationBar_navigates_pages_and_formats_summary() => Sta.Run(() =>
    {
        var pagination = new PaginationBar
        {
            TotalPages = 10,
            CurrentPage = 1,
            PageSize = 10,
            TotalItems = 95
        };

        Sta.Layout(pagination);

        Assert.Equal(1, pagination.CurrentPage);
        Assert.Equal(10, pagination.TotalPages);
        Assert.Equal("Showing 1–10 of 95 results", pagination.SummaryText);

        // Next page
        pagination.GoToNext();
        Assert.Equal(2, pagination.CurrentPage);
        Assert.Equal("Showing 11–20 of 95 results", pagination.SummaryText);

        // Go to last page
        pagination.CurrentPage = 10;
        Assert.Equal(10, pagination.CurrentPage);
        Assert.Equal("Showing 91–95 of 95 results", pagination.SummaryText);

        // Clamping check
        pagination.CurrentPage = 999;
        Assert.Equal(10, pagination.CurrentPage);

        pagination.CurrentPage = -5;
        Assert.Equal(1, pagination.CurrentPage);
    });
    [Fact]
    public void SegmentedItem_maintains_consistent_font_weight_when_selected() => Sta.Run(() =>
    {
        var control = new SegmentedControl();
        var item = new SegmentedItem { Content = "Test" };
        control.Items.Add(item);
        Sta.Layout(control);
        var unselectedWeight = item.FontWeight;

        item.IsSelected = true;
        Sta.Layout(control);
        var selectedWeight = item.FontWeight;

        Assert.Equal(FontWeights.SemiBold, unselectedWeight);
        Assert.Equal(FontWeights.SemiBold, selectedWeight);
    });

    [Fact]
    public void CustomWindow_template_and_properties() => Sta.Run(() =>
    {
        var window = new CustomWindow
        {
            Title = "My Application",
            TitleBarHeight = 38.0,
            IsTitleBarVisible = true,
            ShowMinimizeButton = true,
            ShowMaximizeButton = true,
            ShowCloseButton = true
        };
        window.Resources.MergedDictionaries.Add(new CleanThemeResources());
        window.Show();
        Sta.DoEvents();

        Assert.Equal(38.0, window.TitleBarHeight);
        Assert.True(window.IsTitleBarVisible);
        Assert.True(window.ShowMinimizeButton);
        Assert.True(window.ShowMaximizeButton);
        Assert.True(window.ShowCloseButton);

        var titleBar = (FrameworkElement)window.Template.FindName(CustomWindow.PartTitleBar, window);
        Assert.NotNull(titleBar);
        Assert.Equal(Visibility.Visible, titleBar.Visibility);

        var minBtn = (Button)window.Template.FindName(CustomWindow.PartMinimizeButton, window);
        Assert.NotNull(minBtn);
        Assert.Equal(Visibility.Visible, minBtn.Visibility);

        var maxBtn = (Button)window.Template.FindName(CustomWindow.PartMaximizeButton, window);
        Assert.NotNull(maxBtn);
        Assert.Equal(Visibility.Visible, maxBtn.Visibility);

        var closeBtn = (Button)window.Template.FindName(CustomWindow.PartCloseButton, window);
        Assert.NotNull(closeBtn);
        Assert.Equal(Visibility.Visible, closeBtn.Visibility);

        window.Close();
    });

    [Fact]
    public void CustomWindow_titlebar_controls_and_slots() => Sta.Run(() =>
    {
        bool leftClicked = false;
        bool centerClicked = false;
        bool rightClicked = false;

        var leftButton = new Button { Content = "Back" };
        leftButton.Click += (_, _) => leftClicked = true;

        var searchBox = new TextBox { Text = "Search query" };
        var centerButton = new Button { Content = "Filter" };
        centerButton.Click += (_, _) => centerClicked = true;
        var centerPanel = new StackPanel { Orientation = Orientation.Horizontal };
        centerPanel.Children.Add(searchBox);
        centerPanel.Children.Add(centerButton);

        var rightButton = new Button { Content = "Theme" };
        rightButton.Click += (_, _) => rightClicked = true;

        var window = new CustomWindow
        {
            Title = "Slot Test Application",
            IsTitleVisible = false,
            TitleBarLeftContent = leftButton,
            TitleBarContent = centerPanel,
            TitleBarRightContent = rightButton
        };
        window.Resources.MergedDictionaries.Add(new CleanThemeResources());
        window.Show();
        Sta.DoEvents();

        Assert.False(window.IsTitleVisible);
        Assert.Same(leftButton, window.TitleBarLeftContent);
        Assert.Same(centerPanel, window.TitleBarContent);
        Assert.Same(rightButton, window.TitleBarRightContent);

        var leftPresenter = (ContentPresenter)window.Template.FindName(CustomWindow.PartTitleBarLeftContent, window);
        var centerPresenter = (ContentPresenter)window.Template.FindName(CustomWindow.PartTitleBarContent, window);
        var rightPresenter = (ContentPresenter)window.Template.FindName(CustomWindow.PartTitleBarRightContent, window);

        Assert.NotNull(leftPresenter);
        Assert.NotNull(centerPresenter);
        Assert.NotNull(rightPresenter);

        // Verify hit-test visibility in WindowChrome is true
        Assert.True(System.Windows.Shell.WindowChrome.GetIsHitTestVisibleInChrome(leftPresenter));
        Assert.True(System.Windows.Shell.WindowChrome.GetIsHitTestVisibleInChrome(centerPresenter));
        Assert.True(System.Windows.Shell.WindowChrome.GetIsHitTestVisibleInChrome(rightPresenter));
        Assert.True(System.Windows.Shell.WindowChrome.GetIsHitTestVisibleInChrome(leftButton));
        Assert.True(System.Windows.Shell.WindowChrome.GetIsHitTestVisibleInChrome(searchBox));
        Assert.True(System.Windows.Shell.WindowChrome.GetIsHitTestVisibleInChrome(rightButton));

        // Verify controls receive clicks
        leftButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        centerButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        rightButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        Assert.True(leftClicked);
        Assert.True(centerClicked);
        Assert.True(rightClicked);

        window.Close();
    });
}
