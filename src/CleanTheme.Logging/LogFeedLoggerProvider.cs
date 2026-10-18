using Microsoft.Extensions.Logging;

namespace CleanTheme.Logging;

/// <summary>Writes <see cref="ILogger"/> output into a <see cref="LogFeed"/>. Level filtering is the logging configuration's job.</summary>
[ProviderAlias("LogFeed")]
public sealed class LogFeedLoggerProvider(LogFeed feed) : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new LogFeedLogger(categoryName, feed);

    public void Dispose()
    {
    }

    private sealed class LogFeedLogger(string category, LogFeed feed) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }
            var message = formatter(state, exception);
            if (exception != null)
            {
                message = $"{message}{Environment.NewLine}{exception}";
            }
            feed.Post(new LogEntry(DateTime.UtcNow, logLevel, null, message) { Category = category });
        }
    }
}
