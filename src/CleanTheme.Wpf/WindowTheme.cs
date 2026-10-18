using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace CleanTheme.Wpf;

/// <summary>
/// Makes a window's title bar follow <see cref="ThemeManager"/> (dark caption in dark mode, Windows 10 2004+).
/// <c>CleanWindow</c> and <c>CleanDialog</c> set <see cref="FollowThemeProperty"/>; set it yourself on other windows.
/// </summary>
public static partial class WindowTheme
{
    public static readonly DependencyProperty FollowThemeProperty = DependencyProperty.RegisterAttached(
        "FollowTheme", typeof(bool), typeof(WindowTheme), new FrameworkPropertyMetadata(false, OnFollowThemeChanged));

    // Keeps each window's target alive exactly as long as the window (ThemeManager holds targets weakly).
    private static readonly ConditionalWeakTable<Window, TitleBarTarget> TargetsByWindow = new();

    private const int DwmUseImmersiveDarkMode = 20;

    public static bool GetFollowTheme(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);
        return (bool)window.GetValue(FollowThemeProperty);
    }

    public static void SetFollowTheme(Window window, bool value)
    {
        ArgumentNullException.ThrowIfNull(window);
        window.SetValue(FollowThemeProperty, value);
    }

    private static void OnFollowThemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Window window || e.NewValue is not true)
        {
            return;
        }
        var target = TargetsByWindow.GetValue(window, w => new TitleBarTarget(w));
        ThemeManager.Register(target);
        if (new WindowInteropHelper(window).Handle != IntPtr.Zero)
        {
            target.Apply(ThemeManager.ActualMode);
        }
        else
        {
            window.SourceInitialized += (_, _) => target.Apply(ThemeManager.ActualMode);
        }
    }

    [LibraryImport("dwmapi.dll")]
    private static partial int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

    private sealed class TitleBarTarget(Window window) : IThemeTarget
    {
        private readonly WeakReference<Window> _window = new(window);

        public void Apply(CleanThemeMode actualMode)
        {
            if (!_window.TryGetTarget(out var w))
            {
                return;
            }
            if (!w.Dispatcher.CheckAccess())
            {
                w.Dispatcher.BeginInvoke(() => Apply(actualMode));
                return;
            }
            var handle = new WindowInteropHelper(w).Handle;
            if (handle == IntPtr.Zero || !GetFollowTheme(w))
            {
                return;
            }
            var dark = actualMode == CleanThemeMode.Dark ? 1 : 0;
            // Fails harmlessly on Windows versions without the attribute.
            _ = DwmSetWindowAttribute(handle, DwmUseImmersiveDarkMode, ref dark, sizeof(int));
        }
    }
}
