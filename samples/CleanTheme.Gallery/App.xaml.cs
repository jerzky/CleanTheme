using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CleanTheme.Gallery;

/// <summary>
/// Starts the host, shows the main window, and on close stops the host (ending its background services) before
/// shutting down.
/// </summary>
public partial class App : Application
{
    private readonly IHost _host;

    public App(IHost host)
    {
        _host = host;
        // Shut down only after the host has stopped (see OnMainWindowClosed).
        ShutdownMode = ShutdownMode.OnExplicitShutdown;
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        await _host.StartAsync();

        var window = _host.Services.GetRequiredService<MainWindow>();
        window.Closed += OnMainWindowClosed;
        MainWindow = window;
        window.Show();
    }

    private async void OnMainWindowClosed(object? sender, EventArgs e)
    {
        await _host.StopAsync();
        Shutdown();
    }
}
