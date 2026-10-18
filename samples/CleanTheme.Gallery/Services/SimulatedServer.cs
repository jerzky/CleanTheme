using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CleanTheme.Gallery.Services;

/// <summary>Stands in for a server loop: logs its ticks, the odd slow one, and periodic snapshots. Logs from a pool thread.</summary>
public sealed partial class SimulatedServer(ILogger<SimulatedServer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        LogListening(logger, 9050);
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
                    LogSnapshot(logger, 40 + (tick % 7));
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Host shutdown.
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Listening on UDP port {Port}")]
    private static partial void LogListening(ILogger logger, int port);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Tick {Tick} took {TickMs:F1} ms")]
    private static partial void LogTick(ILogger logger, int tick, double tickMs);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Slow tick {Tick}: {TickMs:F1} ms (budget 16 ms)")]
    private static partial void LogSlowTick(ILogger logger, int tick, double tickMs);

    [LoggerMessage(Level = LogLevel.Information, Message = "Snapshot published: {Entities} entities")]
    private static partial void LogSnapshot(ILogger logger, int entities);
}
