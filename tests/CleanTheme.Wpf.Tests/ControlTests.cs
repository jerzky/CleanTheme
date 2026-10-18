using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using CleanTheme.Wpf.Controls;

namespace CleanTheme.Wpf.Tests;

public sealed class ControlTests
{
    private static readonly string[] TimeScales = ["1x", "2x", "4x"];

    [Theory]
    [InlineData(StatusState.Neutral, "#EDF0F4", "#5A6576")]
    [InlineData(StatusState.Info, "#E7EEF7", "#1D4B87")]
    [InlineData(StatusState.Positive, "#E5F3EB", "#16794A")]
    [InlineData(StatusState.Warning, "#FCF1D5", "#8F6200")]
    [InlineData(StatusState.Negative, "#FCEBE9", "#B42318")]
    public void StatusPill_colors_follow_its_state(StatusState state, string background, string foreground) => Sta.Run(() =>
    {
        var pill = new StatusPill { State = state, Content = "x" };
        Sta.Layout(pill);

        Assert.Equal(ColorOf(background), ((SolidColorBrush)pill.Background).Color);
        Assert.Equal(ColorOf(foreground), ((SolidColorBrush)pill.Foreground).Color);
    });

    [Fact]
    public void Card_hides_the_title_row_without_header_or_actions() => Sta.Run(() =>
    {
        var card = new Card { Content = "body" };
        Sta.Layout(card);
        var titleRow = (FrameworkElement)card.Template.FindName("TitleRow", card);

        Assert.Equal(Visibility.Collapsed, titleRow.Visibility);

        card.Header = "Title";
        Assert.Equal(Visibility.Visible, titleRow.Visibility);

        card.Header = null;
        card.Actions = new Button();
        Assert.Equal(Visibility.Visible, titleRow.Visibility);
    });

    [Fact]
    public void MetricTile_hides_the_unit_when_there_is_none() => Sta.Run(() =>
    {
        var tile = new MetricTile { Label = "Tick", Value = 12.4 };
        Sta.Layout(tile);
        var unit = (FrameworkElement)tile.Template.FindName("UnitText", tile);

        Assert.Equal(Visibility.Collapsed, unit.Visibility);

        tile.Unit = "ms";
        Assert.Equal(Visibility.Visible, unit.Visibility);
    });

    [Fact]
    public void SegmentedControl_wraps_items_in_SegmentedItems_and_selects_one() => Sta.Run(() =>
    {
        var segments = new SegmentedControl { ItemsSource = TimeScales, SelectedIndex = 1 };
        Sta.Layout(segments);

        var containers = Enumerable.Range(0, 3)
            .Select(i => segments.ItemContainerGenerator.ContainerFromIndex(i))
            .ToArray();

        Assert.All(containers, container => Assert.IsType<SegmentedItem>(container));
        Assert.Equal([false, true, false], containers.Select(c => ((SegmentedItem)c).IsSelected));
        Assert.Equal(SelectionMode.Single, segments.SelectionMode);
    });

    [Fact]
    public void Horizontal_ScrollBar_is_thin_and_uses_the_horizontal_template() => Sta.Run(() =>
    {
        var theme = new CleanThemeResources();
        var bar = new ScrollBar { Orientation = Orientation.Horizontal };
        bar.Resources.MergedDictionaries.Add(theme);
        Sta.Layout(bar, 200, 50);

        Assert.Equal(10, bar.Height);
        Assert.Same(theme["HorizontalScrollBarTemplate"], bar.Template);
    });

    [Theory]
    [InlineData(Orientation.Vertical)]
    [InlineData(Orientation.Horizontal)]
    public void ScrollBar_thumb_keeps_its_minimum_length_over_long_content(Orientation orientation) => Sta.Run(() =>
    {
        var bar = new ScrollBar { Orientation = orientation, Minimum = 0, Maximum = 10_000, ViewportSize = 10, Value = 10_000 };
        bar.Resources.MergedDictionaries.Add(new CleanThemeResources());
        var vertical = orientation == Orientation.Vertical;
        Sta.Layout(bar, vertical ? 10 : 200, vertical ? 200 : 10);

        var track = Assert.IsType<ScrollBarTrack>(bar.Track);
        var slot = LayoutInformation.GetLayoutSlot(track.Thumb);

        Assert.Equal(track.MinThumbLength, vertical ? slot.Height : slot.Width, 3);
        // At the maximum the thumb sits at the far end of the track, not past it.
        Assert.Equal(200, vertical ? slot.Bottom : slot.Right, 3);
        // Dragging the full travel covers the full range.
        var full = track.ValueFromDistance(vertical ? 0 : 200 - track.MinThumbLength, vertical ? 200 - track.MinThumbLength : 0);
        Assert.Equal(10_000, Math.Abs(full), 3);
    });

    private static Color ColorOf(string hex) => (Color)ColorConverter.ConvertFromString(hex);
}
