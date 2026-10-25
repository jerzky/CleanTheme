using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CleanTheme.Gallery.Services;

/// <summary>Stands in for an API server loop: logs requests, the odd slow query, and periodic cache snapshots.</summary>
public sealed partial class SimulatedServer(ILogger<SimulatedServer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        LogListening(logger, 8080);
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        var tick = 0;
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                tick++;
                var tickMs = tick % 9 == 0 ? 21.7 : 8 + (tick % 5);
                if (tickMs > 16)
                {
                    LogSlowTick(logger, tick, tickMs);
                }
                else
                {
                    LogTick(logger, tick, tickMs);
                }
                if (tick % 5 == 0)
                {
                    LogSnapshot(logger, 140 + (tick % 17));
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Host shutdown.
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Listening on HTTPS port {Port}")]
    private static partial void LogListening(ILogger logger, int port);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Request #{Tick} processed in {TickMs:F1} ms")]
    private static partial void LogTick(ILogger logger, int tick, double tickMs);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Slow request #{Tick}: {TickMs:F1} ms (target 16 ms)")]
    private static partial void LogSlowTick(ILogger logger, int tick, double tickMs);

    [LoggerMessage(Level = LogLevel.Information, Message = "Cache snapshot refreshed: {Entities} active records")]
    private static partial void LogSnapshot(ILogger logger, int entities);
}
