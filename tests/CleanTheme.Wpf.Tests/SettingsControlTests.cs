using System.Windows;
using System.Windows.Controls;
using CleanTheme.Wpf.Controls;

namespace CleanTheme.Wpf.Tests;

public sealed class SettingsControlTests
{
    [Fact]
    public void SettingRow_has_expected_defaults() => Sta.Run(() =>
    {
        var row = new SettingRow();
        Assert.Null(row.Header);
        Assert.Null(row.Description);
        Assert.False(row.IsRequired);
        Assert.True(row.IsMatch);
        Assert.Equal(new GridLength(260), row.HeaderWidth);
    });

    [Fact]
    public void SettingRow_Filter_matches_header_and_description() => Sta.Run(() =>
    {
        var row = new SettingRow
        {
            Header = "Email Address",
            Description = "We will never share your email with third parties.",
            Keywords = "mail, contact"
        };

        Assert.True(row.Filter(""));
        Assert.True(row.IsMatch);

        Assert.True(row.Filter("email"));
        Assert.True(row.IsMatch);

        Assert.True(row.Filter("third parties"));
        Assert.True(row.IsMatch);

        Assert.True(row.Filter("contact"));
        Assert.True(row.IsMatch);

        Assert.False(row.Filter("nonexistent-xyz"));
        Assert.False(row.IsMatch);
    });

    [Fact]
    public void SettingsGroup_Filter_matches_group_header_or_children() => Sta.Run(() =>
    {
        var group = new SettingsGroup { Header = "Personal info" };
        var row1 = new SettingRow { Header = "First Name" };
        var row2 = new SettingRow { Header = "Last Name" };
        group.Items.Add(row1);
        group.Items.Add(row2);

        // Group header match reveals all child rows
        Assert.True(group.Filter("Personal"));
        Assert.True(group.IsMatch);
        Assert.True(row1.IsMatch);
        Assert.True(row2.IsMatch);

        // Specific child match reveals group and matching child only
        Assert.True(group.Filter("Last"));
        Assert.True(group.IsMatch);
        Assert.False(row1.IsMatch);
        Assert.True(row2.IsMatch);

        // No match
        Assert.False(group.Filter("Billing"));
        Assert.False(group.IsMatch);
    });

    [Fact]
    public void SettingsCategory_Filter_propagates_to_groups_and_rows() => Sta.Run(() =>
    {
        var cat = new SettingsCategory { Header = "Account" };
        var group = new SettingsGroup { Header = "Security" };
        var row = new SettingRow { Header = "Two-Factor Auth" };
        group.Items.Add(row);
        cat.Items.Add(group);

        Assert.True(cat.Filter("Two-Factor"));
        Assert.True(cat.IsMatch);
        Assert.True(group.IsMatch);
        Assert.True(row.IsMatch);

        Assert.False(cat.Filter("NonexistentQuery"));
        Assert.False(cat.IsMatch);
    });

    [Fact]
    public void SearchableSettingsView_Filter_updates_HasMatches_and_IsSearching() => Sta.Run(() =>
    {
        var view = new SearchableSettingsView();
        var cat = new SettingsCategory { Header = "Profile" };
        var group = new SettingsGroup { Header = "Details" };
        var row = new SettingRow { Header = "Username" };
        group.Items.Add(row);
        cat.Items.Add(group);
        view.Items.Add(cat);

        Assert.True(view.HasMatches);
        Assert.False(view.IsSearching);

        view.SearchText = "User";
        Assert.True(view.IsSearching);
        Assert.True(view.HasMatches);

        view.SearchText = "XYZ_NOT_FOUND";
        Assert.True(view.IsSearching);
        Assert.False(view.HasMatches);

        view.SearchText = "";
        Assert.False(view.IsSearching);
        Assert.True(view.HasMatches);
    });

    [Fact]
    public void SearchableSettingsView_layouts_without_exceptions() => Sta.Run(() =>
    {
        var view = new SearchableSettingsView { Title = "App Preferences" };
        var cat1 = new SettingsCategory { Header = "General" };
        var grp1 = new SettingsGroup { Header = "Theme" };
        grp1.Items.Add(new SettingRow { Header = "Dark Mode" });
        cat1.Items.Add(grp1);

        var cat2 = new SettingsCategory { Header = "Notifications", Badge = "3" };
        var grp2 = new SettingsGroup { Header = "Email Alerts" };
        grp2.Items.Add(new SettingRow { Header = "Marketing" });
        cat2.Items.Add(grp2);

        view.Items.Add(cat1);
        view.Items.Add(cat2);

        Sta.Layout(view);
        Assert.Equal(0, view.SelectedIndex);

        // Switch tab
        view.SelectedIndex = 1;
        Sta.Layout(view);
        Assert.Equal(1, view.SelectedIndex);

        // Switch back
        view.SelectedIndex = 0;
        Sta.Layout(view);
        Assert.Equal(0, view.SelectedIndex);

        // Search
        view.SearchText = "Marketing";
        Sta.Layout(view);
        Assert.True(view.IsSearching);
        Assert.True(view.HasMatches);

        view.SearchText = "";
        Sta.Layout(view);
        Assert.False(view.IsSearching);
    });

    [Fact]
    public void SettingsCategory_selection_mutually_exclusive_on_click() => Sta.Run(() =>
    {
        var view = new SearchableSettingsView { Title = "Preferences" };
        var cat1 = new SettingsCategory { Header = "General" };
        var cat2 = new SettingsCategory { Header = "Profile" };
        var cat3 = new SettingsCategory { Header = "Notifications" };

        view.Items.Add(cat1);
        view.Items.Add(cat2);
        view.Items.Add(cat3);

        Sta.Layout(view);

        // Initially first category is selected
        Assert.Equal(0, view.SelectedIndex);
        Assert.True(cat1.IsSelected);
        Assert.False(cat2.IsSelected);
        Assert.False(cat3.IsSelected);

        // Click second category
        cat2.RaiseEvent(new System.Windows.Input.MouseButtonEventArgs(
            System.Windows.Input.Mouse.PrimaryDevice, 0, System.Windows.Input.MouseButton.Left)
        {
            RoutedEvent = UIElement.MouseLeftButtonDownEvent
        });
        Sta.Layout(view);

        // Only second category should be selected
        Assert.False(cat1.IsSelected);
        Assert.True(cat2.IsSelected);
        Assert.False(cat3.IsSelected);
        Assert.Equal(1, view.SelectedIndex);
        Assert.Same(cat2, view.SelectedItem);

        // Click third category
        cat3.RaiseEvent(new System.Windows.Input.MouseButtonEventArgs(
            System.Windows.Input.Mouse.PrimaryDevice, 0, System.Windows.Input.MouseButton.Left)
        {
            RoutedEvent = UIElement.MouseLeftButtonDownEvent
        });
        Sta.Layout(view);

        // Only third category should be selected
        Assert.False(cat1.IsSelected);
        Assert.False(cat2.IsSelected);
        Assert.True(cat3.IsSelected);
        Assert.Equal(2, view.SelectedIndex);
        Assert.Same(cat3, view.SelectedItem);
    });

    [Fact]
    public void SettingsCategory_supports_keyboard_navigation() => Sta.Run(() =>
    {
        var view = new SearchableSettingsView { Title = "Preferences" };
        var cat1 = new SettingsCategory { Header = "General" };
        var cat2 = new SettingsCategory { Header = "Security" };

        view.Items.Add(cat1);
        view.Items.Add(cat2);
        Sta.Layout(view);

        Assert.True(cat1.IsSelected);
        Assert.Equal(0, view.SelectedIndex);

        // Press Right arrow on cat1
        using (var source = new System.Windows.Interop.HwndSource(0, 0, 0, 0, 0, "", IntPtr.Zero))
        {
            cat1.RaiseEvent(new System.Windows.Input.KeyEventArgs(
                System.Windows.Input.Keyboard.PrimaryDevice,
                source,
                0,
                System.Windows.Input.Key.Right)
            {
                RoutedEvent = System.Windows.Input.Keyboard.KeyDownEvent
            });
        }
        Sta.Layout(view);

        Assert.False(cat1.IsSelected);
        Assert.True(cat2.IsSelected);
        Assert.Equal(1, view.SelectedIndex);
    });

    [Fact]
    public void SettingsCategory_automation_peer_reports_TabItem_and_SelectionItemPattern() => Sta.Run(() =>
    {
        var cat = new SettingsCategory { Header = "Notifications" };
        Sta.Layout(cat);

        var peer = System.Windows.Automation.Peers.UIElementAutomationPeer.CreatePeerForElement(cat);
        Assert.NotNull(peer);
        Assert.Equal("Notifications", peer.GetName());
        Assert.Equal(System.Windows.Automation.Peers.AutomationControlType.TabItem, peer.GetAutomationControlType());

        var pattern = peer.GetPattern(System.Windows.Automation.Peers.PatternInterface.SelectionItem)
            as System.Windows.Automation.Provider.ISelectionItemProvider;
        Assert.NotNull(pattern);
        Assert.False(pattern.IsSelected);

        pattern.Select();
        Assert.True(cat.IsSelected);
        Assert.True(pattern.IsSelected);
    });
}
