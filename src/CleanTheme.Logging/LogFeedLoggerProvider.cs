using Microsoft.Extensions.Logging;

namespace CleanTheme.Logging;

/// <summary>
/// Writes <see cref="ILogger"/> output into a <see cref="LogFeed"/>. Level filtering is the logging configuration's job
/// (provider alias <c>LogFeed</c>).
/// </summary>
/// <remarks>
/// Scopes are honored: a <see cref="SourceScopeKey"/> value in any enclosing scope becomes the line's
/// <see cref="LogEntry.Source"/> (the innermost wins), e.g.
/// <c>using (logger.BeginScope(new Dictionary&lt;string, object&gt; { ["Source"] = "UAV-1" }))</c> or a message-template
/// scope such as <c>LoggerMessage.DefineScope&lt;string&gt;("{Source}")</c>.
/// </remarks>
[ProviderAlias("LogFeed")]
public sealed class LogFeedLoggerProvider(LogFeed feed) : ILoggerProvider, ISupportExternalScope
{
    /// <summary>Scope key whose value becomes <see cref="LogEntry.Source"/>.</summary>
    public const string SourceScopeKey = "Source";

    // The host's scope provider (shared with the other providers) once SetScopeProvider is called; own scopes until then.
    private volatile IExternalScopeProvider _scopeProvider = new LoggerExternalScopeProvider();

    public ILogger CreateLogger(string categoryName) => new LogFeedLogger(categoryName, feed, this);

    public void SetScopeProvider(IExternalScopeProvider scopeProvider)
    {
        ArgumentNullException.ThrowIfNull(scopeProvider);
        _scopeProvider = scopeProvider;
    }

    public void Dispose()
    {
    }

    private static string? FindSource(IExternalScopeProvider scopes)
    {
        var found = new SourceHolder();
        scopes.ForEachScope(static (scope, holder) =>
        {
            if (scope is IEnumerable<KeyValuePair<string, object?>> values)
            {
                foreach (var (key, value) in values)
                {
                    if (key == SourceScopeKey && value?.ToString() is { Length: > 0 } text)
                    {
                        holder.Source = text; // Scopes run outermost first, so the innermost one wins.
                    }
                }
            }
        }, found);
        return found.Source;
    }

    private sealed class SourceHolder
    {
        public string? Source { get; set; }
    }

    private sealed class LogFeedLogger(string category, LogFeed feed, LogFeedLoggerProvider provider) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => provider._scopeProvider.Push(state);

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
            var source = FindSource(provider._scopeProvider);
            feed.Post(new LogEntry(DateTime.UtcNow, logLevel, source, message) { Category = category });
        }
    }
}
