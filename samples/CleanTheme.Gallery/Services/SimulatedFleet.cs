using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CleanTheme.Gallery.Services;

/// <summary>
/// Stands in for a supervisor of several units. Each unit's lines are logged inside a <c>{Source}</c> scope, so the
/// LogView shows the callsign as the line's origin and the filter popout can narrow to one unit.
/// </summary>
public sealed partial class SimulatedFleet(ILogger<SimulatedFleet> logger) : BackgroundService
{
    private static readonly string[] Callsigns = ["ALPHA-1", "BRAVO-2", "CHARLIE-3"];

    // A scope whose "Source" value becomes LogEntry.Source (see LogFeedLoggerProvider.SourceScopeKey).
    private static readonly Func<ILogger, string, IDisposable?> UnitScope = LoggerMessage.DefineScope<string>("{Source}");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        foreach (var callsign in Callsigns)
        {
            using (UnitScope(logger, callsign))
            {
                LogSpawned(logger);
            }
        }

        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(1300));
        var step = 0;
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                step++;
                using (UnitScope(logger, Callsigns[step % Callsigns.Length]))
                {
                    if (step % 11 == 0)
                    {
                        LogFuelLow(logger, 18);
                    }
                    else if (step % 2 == 0)
                    {
                        LogWaypoint(logger, step / 2);
                    }
                    else
                    {
                        LogHeading(logger, (step * 37) % 360);
                    }
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Host shutdown.
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Spawned")]
    private static partial void LogSpawned(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Waypoint {Waypoint} reached")]
    private static partial void LogWaypoint(ILogger logger, int waypoint);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Heading {HeadingDeg:000}")]
    private static partial void LogHeading(ILogger logger, int headingDeg);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Fuel low: {Percent}%")]
    private static partial void LogFuelLow(ILogger logger, int percent);
}
