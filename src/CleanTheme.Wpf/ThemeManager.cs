using Microsoft.Win32;

namespace CleanTheme.Wpf;

/// <summary>Light or dark, or whatever Windows uses for apps.</summary>
public enum CleanThemeMode
{
    Light,
    Dark,

    /// <summary>Follows the Windows "app mode" setting, live.</summary>
    System,
}

/// <summary>Something that recolors when the theme changes: a <see cref="CleanThemeResources"/> or a window title bar.</summary>
internal interface IThemeTarget
{
    /// <summary>Applies <paramref name="actualMode"/> (Light or Dark); may be called from any thread.</summary>
    void Apply(CleanThemeMode actualMode);
}

/// <summary>
/// The app-wide theme mode. Setting <see cref="Mode"/> recolors every <see cref="CleanThemeResources"/> (and windows using
/// <c>CleanWindow</c> / <c>CleanDialog</c>) live; <see cref="CleanThemeMode.System"/> follows the Windows setting as it changes.
/// </summary>
/// <remarks>
/// Thread-safe. Targets are recolored on their own dispatchers. <see cref="ActualModeChanged"/> is raised on the thread
/// that changed <see cref="Mode"/>, or on a system-events thread when Windows changes app mode; UI subscribers marshal
/// to their dispatcher.
/// </remarks>
public static class ThemeManager
{
    private const string PersonalizeKey = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

    private static readonly object Gate = new();
    private static readonly List<WeakReference<IThemeTarget>> Targets = [];
    private static CleanThemeMode _mode = CleanThemeMode.Light;
    private static CleanThemeMode _actualMode = CleanThemeMode.Light;
    private static bool _listeningToSystem;

    /// <summary>Raised when <see cref="ActualMode"/> changes (see the class remarks for the thread).</summary>
    public static event EventHandler? ActualModeChanged;

    /// <summary>Light (the default), Dark, or System.</summary>
    public static CleanThemeMode Mode
    {
        get
        {
            lock (Gate)
            {
                return _mode;
            }
        }
        set
        {
            if (!Enum.IsDefined(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
            lock (Gate)
            {
                _mode = value;
                if (value == CleanThemeMode.System && !_listeningToSystem)
                {
                    SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
                    _listeningToSystem = true;
                }
                else if (value != CleanThemeMode.System && _listeningToSystem)
                {
                    SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;
                    _listeningToSystem = false;
                }
            }
            Refresh();
        }
    }

    /// <summary>The mode in effect: Light or Dark (System resolved).</summary>
    public static CleanThemeMode ActualMode
    {
        get
        {
            lock (Gate)
            {
                return _actualMode;
            }
        }
    }

    /// <summary>Whether Windows is set to dark mode for apps.</summary>
    public static bool IsSystemDark()
    {
        using var key = Registry.CurrentUser.OpenSubKey(PersonalizeKey);
        return key?.GetValue("AppsUseLightTheme") is int useLight && useLight == 0;
    }

    internal static void Register(IThemeTarget target)
    {
        lock (Gate)
        {
            Targets.RemoveAll(reference => !reference.TryGetTarget(out _));
            Targets.Add(new WeakReference<IThemeTarget>(target));
        }
    }

    private static void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        if (e.Category is UserPreferenceCategory.General or UserPreferenceCategory.VisualStyle)
        {
            Refresh();
        }
    }

    /// <summary>Resolves the mode and, if the result changed, recolors every live target.</summary>
    private static void Refresh()
    {
        var resolved = Mode switch
        {
            CleanThemeMode.System => IsSystemDark() ? CleanThemeMode.Dark : CleanThemeMode.Light,
            var mode => mode,
        };

        IThemeTarget[] targets;
        lock (Gate)
        {
            if (resolved == _actualMode)
            {
                return;
            }
            _actualMode = resolved;
            targets = [.. Targets.Select(reference => reference.TryGetTarget(out var target) ? target : null).OfType<IThemeTarget>()];
        }

        foreach (var target in targets)
        {
            target.Apply(resolved);
        }
        ActualModeChanged?.Invoke(null, EventArgs.Empty);
    }
}
