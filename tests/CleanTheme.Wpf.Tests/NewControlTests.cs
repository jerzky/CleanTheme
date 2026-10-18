using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using CleanTheme.Wpf.Controls;

namespace CleanTheme.Wpf.Tests;

public sealed class NewControlTests
{
    [Fact]
    public void CheckBox_fills_with_the_accent_when_checked() => Sta.Run(() =>
    {
        var theme = new CleanThemeResources();
        var box = new CheckBox { Content = "Option" };
        box.Resources.MergedDictionaries.Add(theme);
        Sta.Layout(box);
        var fill = (Border)box.Template.FindName("Box", box);

        Assert.Same(theme["Surface"], fill.Background);

        box.IsChecked = true;
        Assert.Same(theme["AccentFill"], fill.Background);
        Assert.Equal(Visibility.Visible, ((FrameworkElement)box.Template.FindName("Check", box)).Visibility);

        box.IsChecked = null;
        Assert.Equal(Visibility.Visible, ((FrameworkElement)box.Template.FindName("Dash", box)).Visibility);
    });

    [Fact]
    public void MenuItem_template_follows_its_role() => Sta.Run(() =>
    {
        var menu = new Menu();
        menu.Resources.MergedDictionaries.Add(new CleanThemeResources());
        var file = new MenuItem { Header = "_File" };
        var recent = new MenuItem { Header = "Recent" };
        recent.Items.Add(new MenuItem { Header = "a.json" });
        file.Items.Add(recent);
        file.Items.Add(new MenuItem { Header = "Exit", InputGestureText = "Alt+F4" });
        menu.Items.Add(file);
        Sta.Layout(menu);

        Assert.Equal(MenuItemRole.TopLevelHeader, file.Role);
        Assert.IsType<Popup>(file.Template.FindName("PART_Popup", file));
        Assert.Equal(MenuItemRole.SubmenuHeader, recent.Role);
    });

    [Fact]
    public void DatePicker_applies_its_template_parts() => Sta.Run(() =>
    {
        var picker = new DatePicker { SelectedDate = new DateTime(2026, 10, 18) };
        picker.Resources.MergedDictionaries.Add(new CleanThemeResources());
        Sta.Layout(picker);

        Assert.IsType<DatePickerTextBox>(picker.Template.FindName("PART_TextBox", picker));
        Assert.IsType<Popup>(picker.Template.FindName("PART_Popup", picker));
    });

    [Fact]
    public void SearchBox_shows_the_placeholder_until_there_is_text_and_clears() => Sta.Run(() =>
    {
        var search = new SearchBox { Placeholder = "Find" };
        Sta.Layout(search);
        var placeholder = (FrameworkElement)search.Template.FindName("Placeholder", search);
        var clear = (ButtonBase)search.Template.FindName("PART_ClearButton", search);

        Assert.Equal(Visibility.Visible, placeholder.Visibility);
        Assert.Equal(Visibility.Collapsed, clear.Visibility);

        search.Text = "tick";
        Assert.Equal(Visibility.Collapsed, placeholder.Visibility);
        Assert.Equal(Visibility.Visible, clear.Visibility);

        clear.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        Assert.Equal("", search.Text);
    });

    [Fact]
    public void ToggleSwitch_moves_the_knob_when_on() => Sta.Run(() =>
    {
        var toggle = new ToggleSwitch { Content = "Auto-scroll" };
        Sta.Layout(toggle);
        var knob = (FrameworkElement)toggle.Template.FindName("Knob", toggle);

        Assert.Equal(HorizontalAlignment.Left, knob.HorizontalAlignment);
        toggle.IsChecked = true;
        Assert.Equal(HorizontalAlignment.Right, knob.HorizontalAlignment);
        Assert.False(toggle.IsThreeState);
    });

    [Fact]
    public void EmptyState_hides_the_parts_it_has_no_value_for() => Sta.Run(() =>
    {
        var empty = new EmptyState { Title = "No nodes running" };
        Sta.Layout(empty);

        Assert.Equal(Visibility.Visible, ((FrameworkElement)empty.Template.FindName("TitleText", empty)).Visibility);
        Assert.Equal(Visibility.Collapsed, ((FrameworkElement)empty.Template.FindName("DescriptionText", empty)).Visibility);
        Assert.Equal(Visibility.Collapsed, ((FrameworkElement)empty.Template.FindName("IconPath", empty)).Visibility);
        Assert.Equal(Visibility.Collapsed, ((FrameworkElement)empty.Template.FindName("Action", empty)).Visibility);

        empty.Content = new Button();
        empty.Icon = Geometry.Parse("M 0,0 L 10,10");
        Assert.Equal(Visibility.Visible, ((FrameworkElement)empty.Template.FindName("Action", empty)).Visibility);
        Assert.Equal(Visibility.Visible, ((FrameworkElement)empty.Template.FindName("IconPath", empty)).Visibility);
    });

    [Fact]
    public void NumericUpDown_clamps_rounds_steps_and_reverts_bad_text() => Sta.Run(() =>
    {
        var number = new NumericUpDown { Minimum = 0, Maximum = 10, Increment = 2.5, DecimalPlaces = 1 };
        Sta.Layout(number);
        var text = (TextBox)number.Template.FindName("PART_TextBox", number);

        number.Value = 42;
        Assert.Equal(10, number.Value);

        number.Value = 3.14159;
        Assert.Equal(3.1, number.Value);
        Assert.Equal(3.1.ToString("F1", CultureInfo.CurrentCulture), text.Text);

        number.StepUp();
        Assert.Equal(5.6, number.Value, 6);
        number.StepDown();
        number.StepDown();
        Assert.Equal(0.6, number.Value, 6);

        number.CommitText("7.25");
        Assert.Equal(7.3, number.Value, 6); // Invariant fallback, rounded to one place.

        number.CommitText("not a number");
        Assert.Equal(7.3, number.Value, 6);
        Assert.Equal(number.FormattedValue, text.Text);

        number.Maximum = 5;
        Assert.Equal(5, number.Value);
    });

    [Fact]
    public void ToastHost_stacks_trims_dismisses_and_expires() => Sta.Run(() =>
    {
        var host = new ToastHost { MaxToasts = 2 };
        Sta.Layout(host);

        host.Show("one", duration: TimeSpan.Zero);
        host.Show("two", StatusState.Warning, TimeSpan.Zero);
        host.Show("three", StatusState.Negative, TimeSpan.Zero);
        Assert.Equal(["two", "three"], host.Toasts.Select(t => t.Message));

        ToastHost.DismissCommand.Execute(host.Toasts[0], host);
        Assert.Equal(["three"], host.Toasts.Select(t => t.Message));

        host.Show("brief", duration: TimeSpan.FromMilliseconds(50));
        Assert.Equal(2, host.Toasts.Count);
        Sta.DoEventsFor(TimeSpan.FromMilliseconds(300));
        Assert.Equal(["three"], host.Toasts.Select(t => t.Message));

        // From another thread: marshalled onto the host's dispatcher.
        var thread = new Thread(() => host.Show("from a worker", duration: TimeSpan.Zero));
        thread.Start();
        thread.Join();
        Sta.DoEvents();
        Assert.Equal("from a worker", host.Toasts[^1].Message);
    });

    [Fact]
    public void CleanDialog_buttons_set_the_result_and_close() => Sta.Run(() =>
    {
        var dialog = CleanMessageBox.Create(null, "Delete the scenario?", "Delete", "Delete Baltic CAP", "Delete", "Cancel", isDanger: true);
        dialog.Resources.MergedDictionaries.Add(new CleanThemeResources());
        var closed = false;
        dialog.Closed += (_, _) => closed = true;
        dialog.Show();
        Sta.DoEvents();
        var primary = (Button)dialog.Template.FindName("PART_PrimaryButton", dialog);
        var secondary = (Button)dialog.Template.FindName("PART_SecondaryButton", dialog);

        Assert.Equal((Color)ColorConverter.ConvertFromString("#B42318"), ((SolidColorBrush)primary.Background).Color); // DangerPrimaryButton
        Assert.Equal(Visibility.Visible, secondary.Visibility);
        Assert.True(primary.IsDefault);
        Assert.True(secondary.IsCancel);

        primary.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        Assert.True(closed);
        Assert.Equal(CleanDialogResult.Primary, dialog.Result);
    });

    [Fact]
    public void Prompt_dialog_does_not_confirm_blank_text() => Sta.Run(() =>
    {
        var prompt = CleanMessageBox.CreatePrompt(null, "Save scenario", "Name", "   ");
        prompt.Resources.MergedDictionaries.Add(new CleanThemeResources());
        prompt.Show();
        Sta.DoEvents();
        var primary = (Button)prompt.Template.FindName("PART_PrimaryButton", prompt);

        primary.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        Assert.True(prompt.IsVisible);
        Assert.Equal(CleanDialogResult.None, prompt.Result);

        prompt.Text = "Baltic CAP";
        primary.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        Assert.False(prompt.IsVisible);
        Assert.Equal(CleanDialogResult.Primary, prompt.Result);
    });

    [Fact]
    public void Message_box_without_secondary_text_hides_that_button() => Sta.Run(() =>
    {
        var dialog = CleanMessageBox.Create(null, "Saved.", "Scenario", null, "OK", null, isDanger: false);
        dialog.Resources.MergedDictionaries.Add(new CleanThemeResources());
        dialog.Show();
        Sta.DoEvents();

        Assert.Equal(Visibility.Collapsed, ((FrameworkElement)dialog.Template.FindName("PART_SecondaryButton", dialog)).Visibility);
        Assert.Equal(Visibility.Collapsed, ((FrameworkElement)dialog.Template.FindName("HeadingText", dialog)).Visibility);
        dialog.Close();
    });
}
