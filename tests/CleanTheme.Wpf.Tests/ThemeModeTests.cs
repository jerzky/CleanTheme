using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CleanTheme.Wpf.Tests;

[CollectionDefinition(nameof(ThemeModeGroup), DisableParallelization = true)]
public sealed class ThemeModeGroup;

/// <summary>Switches the app-wide <see cref="ThemeManager.Mode"/>, so it runs alone and always restores Light.</summary>
[Collection(nameof(ThemeModeGroup))]
public sealed class ThemeModeTests
{
    [Fact]
    public void Switching_the_mode_recolors_live_elements_and_back() => Sta.Run(() =>
    {
        try
        {
            var surface = new Border();
            surface.SetResourceReference(Border.BackgroundProperty, "Surface");
            var text = new TextBlock();
            text.SetResourceReference(TextBlock.ForegroundProperty, "Text");
            var panel = new StackPanel { Children = { surface, text } };
            Sta.Layout(panel);
            var raised = 0;
            void OnChanged(object? sender, EventArgs e) => raised++;
            ThemeManager.ActualModeChanged += OnChanged;

            Assert.Equal(ColorOf("#FFFFFF"), ColorIn(surface.Background));

            ThemeManager.Mode = CleanThemeMode.Dark;
            Assert.Equal(CleanThemeMode.Dark, ThemeManager.ActualMode);
            Assert.Equal(ColorOf("#1A212B"), ColorIn(surface.Background));
            Assert.Equal(ColorOf("#E6EAF0"), ColorIn(text.Foreground));

            ThemeManager.Mode = CleanThemeMode.Dark; // No change, no event.
            ThemeManager.Mode = CleanThemeMode.Light;
            Assert.Equal(ColorOf("#FFFFFF"), ColorIn(surface.Background));
            Assert.Equal(2, raised);
            ThemeManager.ActualModeChanged -= OnChanged;
        }
        finally
        {
            ThemeManager.Mode = CleanThemeMode.Light;
        }
    });

    [Fact]
    public void A_mode_set_from_another_thread_recolors_on_the_ui_thread() => Sta.Run(() =>
    {
        try
        {
            var surface = new Border();
            surface.SetResourceReference(Border.BackgroundProperty, "Surface");
            Sta.Layout(surface);

            var worker = new Thread(() => ThemeManager.Mode = CleanThemeMode.Dark);
            worker.Start();
            worker.Join();
            Sta.DoEvents();

            Assert.Equal(ColorOf("#1A212B"), ColorIn(surface.Background));
        }
        finally
        {
            ThemeManager.Mode = CleanThemeMode.Light;
        }
    });

    [Fact]
    public void System_mode_follows_the_windows_setting()
    {
        try
        {
            ThemeManager.Mode = CleanThemeMode.System;

            Assert.Equal(ThemeManager.IsSystemDark() ? CleanThemeMode.Dark : CleanThemeMode.Light, ThemeManager.ActualMode);
            Assert.Equal(CleanThemeMode.System, ThemeManager.Mode);
        }
        finally
        {
            ThemeManager.Mode = CleanThemeMode.Light;
        }
    }

    [Fact]
    public void Both_palettes_define_the_same_keys() => Sta.Run(() =>
    {
        var light = Palette(CleanThemeMode.Light);
        var dark = Palette(CleanThemeMode.Dark);

        Assert.Equal(light.Keys.Cast<string>().Order(), dark.Keys.Cast<string>().Order());
    });

    [Theory]
    [InlineData(CleanThemeMode.Light)]
    [InlineData(CleanThemeMode.Dark)]
    public void Palette_text_meets_contrast_targets(CleanThemeMode mode) => Sta.Run(() =>
    {
        var p = Palette(mode);
        double Contrast(string fore, string back) => ContrastRatio(((SolidColorBrush)p[fore]).Color, ((SolidColorBrush)p[back]).Color);

        foreach (var background in Backgrounds)
        {
            Assert.True(Contrast("Text", background) >= 7, $"Text on {background}");
            Assert.True(Contrast("TextMuted", background) >= 4.5, $"TextMuted on {background}");
            Assert.True(Contrast("TextSubtle", background) >= 3, $"TextSubtle on {background}");
            Assert.True(Contrast("Accent", background) >= 4.5, $"Accent on {background}");
            Assert.True(Contrast("Negative", background) >= 4.5, $"Negative on {background}");
            Assert.True(Contrast("Positive", background) >= 4.5, $"Positive on {background}");
            Assert.True(Contrast("Warning", background) >= 4.5, $"Warning on {background}");
        }
        Assert.True(Contrast("OnAccent", "AccentFill") >= 4.5, "OnAccent on AccentFill");
        Assert.True(Contrast("OnAccent", "NegativeFill") >= 4.5, "OnAccent on NegativeFill");
        Assert.True(Contrast("ToolTipText", "ToolTipBackground") >= 7, "ToolTip");
    });

    private static readonly string[] Backgrounds = ["Surface", "SurfaceSunken", "AppBackground"];

    private static ResourceDictionary Palette(CleanThemeMode mode) => new() { Source = CleanThemeResources.PaletteUri(mode) };

    private static Color ColorIn(Brush brush) => ((SolidColorBrush)brush).Color;

    private static Color ColorOf(string hex) => (Color)ColorConverter.ConvertFromString(hex);

    private static double ContrastRatio(Color a, Color b)
    {
        static double Channel(byte c)
        {
            var v = c / 255.0;
            return v <= 0.03928 ? v / 12.92 : Math.Pow((v + 0.055) / 1.055, 2.4);
        }
        static double Luminance(Color c) => (0.2126 * Channel(c.R)) + (0.7152 * Channel(c.G)) + (0.0722 * Channel(c.B));
        var (high, low) = (Math.Max(Luminance(a), Luminance(b)), Math.Min(Luminance(a), Luminance(b)));
        return (high + 0.05) / (low + 0.05);
    }
}
