using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CleanTheme.Gallery.Services;

/// <summary>
/// Stands in for background microservices. Each service's lines are logged inside a <c>{Source}</c> scope, so the
/// LogView shows the service name as the line's origin and the filter popout can narrow to one service.
/// </summary>
public sealed partial class SimulatedWorkspaceActivity(ILogger<SimulatedWorkspaceActivity> logger) : BackgroundService
{
    private static readonly string[] Services = ["auth-service", "billing-pipeline", "analytics-worker"];

    // A scope whose "Source" value becomes LogEntry.Source (see LogFeedLoggerProvider.SourceScopeKey).
    private static readonly Func<ILogger, string, IDisposable?> ServiceScope = LoggerMessage.DefineScope<string>("{Source}");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        foreach (var svc in Services)
        {
            using (ServiceScope(logger, svc))
            {
                LogServiceStarted(logger);
            }
        }

        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(1300));
        var step = 0;
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                step++;
                using (ServiceScope(logger, Services[step % Services.Length]))
                {
                    if (step % 11 == 0)
                    {
                        LogBacklogWarning(logger, 84);
                    }
                    else if (step % 2 == 0)
                    {
                        LogBatchProcessed(logger, step / 2, 120 + (step % 15));
                    }
                    else
                    {
                        LogLatencyCheck(logger, 12 + (step * 3) % 45);
                    }
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Host shutdown.
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Service instance started")]
    private static partial void LogServiceStarted(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Processed batch #{BatchId} ({Records} events)")]
    private static partial void LogBatchProcessed(ILogger logger, int batchId, int records);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Health check latency {LatencyMs} ms")]
    private static partial void LogLatencyCheck(ILogger logger, int latencyMs);

    [LoggerMessage(Level = LogLevel.Warning, Message = "High queue backlog: {Percent}% buffer utilized")]
    private static partial void LogBacklogWarning(ILogger logger, int percent);
}
