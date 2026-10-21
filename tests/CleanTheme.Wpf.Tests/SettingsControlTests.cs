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
}
