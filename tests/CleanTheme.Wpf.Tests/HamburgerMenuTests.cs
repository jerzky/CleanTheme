using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using CleanTheme.Wpf.Controls;

namespace CleanTheme.Wpf.Tests;

public sealed class HamburgerMenuTests
{
    [Fact]
    public void HamburgerMenu_has_expected_defaults() => Sta.Run(() =>
    {
        var menu = new HamburgerMenu();
        Assert.False(menu.IsPaneOpen);
        Assert.Equal(48.0, menu.CompactPaneWidth);
        Assert.Equal(240.0, menu.OpenPaneWidth);
        Assert.True(menu.IsHamburgerVisible);
        Assert.Equal(SelectionMode.Single, menu.SelectionMode);
    });

    [Fact]
    public void HamburgerMenu_raises_open_and_close_events() => Sta.Run(() =>
    {
        var menu = new HamburgerMenu();
        bool openedRaised = false;
        bool closedRaised = false;

        menu.PaneOpened += (_, _) => openedRaised = true;
        menu.PaneClosed += (_, _) => closedRaised = true;

        menu.IsPaneOpen = true;
        Assert.True(openedRaised);
        Assert.False(closedRaised);

        menu.IsPaneOpen = false;
        Assert.True(closedRaised);
    });

    private static readonly string[] SampleItems = ["Item 1", "Item 2"];

    [Fact]
    public void HamburgerMenu_generates_HamburgerMenuItem_containers() => Sta.Run(() =>
    {
        var menu = new HamburgerMenu();
        menu.ItemsSource = SampleItems;
        Sta.Layout(menu);

        var item = menu.ItemContainerGenerator.ContainerFromIndex(0);
        Assert.IsType<HamburgerMenuItem>(item);
    });

    [Fact]
    public void HamburgerMenuItem_executes_command_on_selection() => Sta.Run(() =>
    {
        object? executedParam = null;
        var command = new TestCommand(p => executedParam = p);

        var menu = new HamburgerMenu();
        var item1 = new HamburgerMenuItem { Text = "Dashboard", Command = command, CommandParameter = "dash" };
        var item2 = new HamburgerMenuItem { Text = "Logs" };
        menu.Items.Add(item1);
        menu.Items.Add(item2);
        Sta.Layout(menu);

        menu.SelectedItem = item1;
        Assert.Equal("dash", executedParam);
    });

    [Fact]
    public void HamburgerMenuItem_template_parts_and_selection_work() => Sta.Run(() =>
    {
        var menu = new HamburgerMenu { IsPaneOpen = true };
        var item = new HamburgerMenuItem
        {
            Icon = Geometry.Parse("M 0,0 L 10,10"),
            Text = "Overview",
            Badge = 5
        };
        menu.Items.Add(item);
        Sta.Layout(menu);

        var chrome = (Border)item.Template.FindName("Chrome", item);
        var bar = (Border)item.Template.FindName("SelectionBar", item);
        Assert.NotNull(chrome);
        Assert.NotNull(bar);
        Assert.Equal(Visibility.Collapsed, bar.Visibility);

        item.IsSelected = true;
        Assert.Equal(Visibility.Visible, bar.Visibility);
    });

    [Fact]
    public void HamburgerMenu_button_toggles_pane_in_template() => Sta.Run(() =>
    {
        var menu = new HamburgerMenu { IsPaneOpen = false };
        Sta.Layout(menu);

        var compactBtn = (ButtonBase?)menu.Template.FindName("PART_CompactHamburgerButton", menu);
        Assert.NotNull(compactBtn);

        compactBtn.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        Assert.True(menu.IsPaneOpen);

        var openBtn = (ButtonBase?)menu.Template.FindName("PART_HamburgerButton", menu);
        Assert.NotNull(openBtn);

        openBtn.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        Assert.False(menu.IsPaneOpen);
    });

    [Fact]
    public void ContextMenu_with_separators_can_open_without_exception() => Sta.Run(() =>
    {
        var menu = new ContextMenu();
        menu.Resources.MergedDictionaries.Add(new CleanThemeResources());
        menu.Items.Add(new MenuItem { Header = "Option 1" });
        menu.Items.Add(new Separator());
        menu.Items.Add(new MenuItem { Header = "Option 2" });
        menu.IsOpen = true;
        Sta.DoEventsFor(TimeSpan.FromMilliseconds(50));
        menu.IsOpen = false;
    });

    [Fact]
    public void HamburgerProfileButton_opens_menu_on_click() => Sta.Run(() =>
    {
        var button = new HamburgerProfileButton
        {
            DisplayName = "jens",
            Subtitle = "Pro",
            OpensMenuOnClick = true
        };
        var menu = new ContextMenu();
        menu.Items.Add(new MenuItem { Header = "Settings" });
        button.ContextMenu = menu;
        Sta.Layout(button);

        button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, button));
        Assert.True(menu.IsOpen);
        Assert.Equal(PlacementMode.Top, menu.Placement);

        Sta.DoEventsFor(TimeSpan.FromMilliseconds(50));
    });

    private sealed class TestCommand(Action<object?> execute) : ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => execute(parameter);
    }
}
