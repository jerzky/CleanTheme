using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Threading;

namespace CleanTheme.Wpf.Tests;

/// <summary>Runs test bodies on an STA thread, which WPF elements require.</summary>
internal static class Sta
{
    static Sta()
    {
        // Application's static constructor registers the pack:// scheme; no Application instance is needed.
        _ = Application.Current;
    }

    public static void Run(Action body)
    {
        ExceptionDispatchInfo? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                body();
            }
            catch (Exception ex)
            {
                failure = ExceptionDispatchInfo.Capture(ex);
            }
            finally
            {
                DoEvents();
                Dispatcher.CurrentDispatcher.InvokeShutdown();
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        failure?.Throw();
    }

    /// <summary>Runs the current dispatcher until everything queued above idle priority has run.</summary>
    public static void DoEvents()
    {
        var frame = new DispatcherFrame();
        Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, () => frame.Continue = false);
        Dispatcher.PushFrame(frame);
    }

    /// <summary>Runs the current dispatcher for at least <paramref name="duration"/> (timers, delayed bindings).</summary>
    public static void DoEventsFor(TimeSpan duration)
    {
        var until = DateTime.UtcNow + duration;
        while (DateTime.UtcNow < until)
        {
            DoEvents();
            Thread.Sleep(10);
        }
        DoEvents();
    }

    /// <summary>Gives an element a size and runs layout, so templates apply and item containers exist.</summary>
    public static void Layout(FrameworkElement element, double width = 400, double height = 300)
    {
        // Colors are DynamicResources found through the tree, as under an app that merges the theme.
        if (!element.Resources.MergedDictionaries.OfType<CleanThemeResources>().Any())
        {
            element.Resources.MergedDictionaries.Add(new CleanThemeResources());
        }

        // An element outside a window only looks up its theme (Generic.xaml) style once initialized.
        if (!element.IsInitialized)
        {
            element.BeginInit();
            element.EndInit();
        }
        element.Measure(new Size(width, height));
        element.Arrange(new Rect(0, 0, width, height));
        element.UpdateLayout();
    }
}
