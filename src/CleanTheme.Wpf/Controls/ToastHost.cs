using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace CleanTheme.Wpf.Controls;

/// <summary>One toast: a short message whose <see cref="State"/> picks its accent stripe.</summary>
public sealed class ToastMessage
{
    public required string Message { get; init; }

    public StatusState State { get; init; } = StatusState.Info;
}

/// <summary>
/// Shows short-lived messages stacked in a corner. Put one over the window content (last child of the root grid) and
/// call <see cref="Show"/>; toasts disappear after their duration or when closed.
/// </summary>
/// <remarks><see cref="Show"/> is safe from any thread; it marshals to the host's dispatcher.</remarks>
public sealed class ToastHost : Control
{
    /// <summary>Closes the toast passed as the command parameter.</summary>
    public static RoutedCommand DismissCommand { get; } = new(nameof(DismissCommand), typeof(ToastHost));

    public static readonly DependencyProperty MaxToastsProperty = DependencyProperty.Register(
        nameof(MaxToasts), typeof(int), typeof(ToastHost), new FrameworkPropertyMetadata(4));

    private readonly ObservableCollection<ToastMessage> _toasts = [];

    static ToastHost()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ToastHost), new FrameworkPropertyMetadata(typeof(ToastHost)));
    }

    public ToastHost()
    {
        Toasts = new ReadOnlyObservableCollection<ToastMessage>(_toasts);
        CommandBindings.Add(new CommandBinding(DismissCommand, (_, e) =>
        {
            if (e.Parameter is ToastMessage toast)
            {
                Dismiss(toast);
            }
        }));
    }

    /// <summary>The toasts on screen, oldest first.</summary>
    public ReadOnlyObservableCollection<ToastMessage> Toasts { get; }

    /// <summary>Most toasts on screen at once; the oldest goes when another arrives. 4 by default.</summary>
    public int MaxToasts
    {
        get => (int)GetValue(MaxToastsProperty);
        set => SetValue(MaxToastsProperty, value);
    }

    /// <summary>Shows a toast for <paramref name="duration"/> (4 s by default; <see cref="TimeSpan.Zero"/> keeps it until closed).</summary>
    public void Show(string message, StatusState state = StatusState.Info, TimeSpan? duration = null)
    {
        ArgumentNullException.ThrowIfNull(message);
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.BeginInvoke(() => Show(message, state, duration));
            return;
        }

        var toast = new ToastMessage { Message = message, State = state };
        _toasts.Add(toast);
        while (_toasts.Count > Math.Max(1, MaxToasts))
        {
            _toasts.RemoveAt(0);
        }

        var lifetime = duration ?? TimeSpan.FromSeconds(4);
        if (lifetime > TimeSpan.Zero)
        {
            var timer = new DispatcherTimer(DispatcherPriority.Normal, Dispatcher) { Interval = lifetime };
            timer.Tick += (_, _) =>
            {
                timer.Stop();
                Dismiss(toast);
            };
            timer.Start();
        }
    }

    /// <summary>Removes a toast now.</summary>
    public void Dismiss(ToastMessage toast)
    {
        VerifyAccess();
        _toasts.Remove(toast);
    }
}
