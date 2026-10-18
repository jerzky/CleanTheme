using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace CleanTheme.Logging;

/// <summary>Registration of the in-app log.</summary>
public static class LogFeedLoggingExtensions
{
    /// <summary>
    /// Sends <see cref="ILogger"/> output to a <see cref="LogFeed"/> singleton (resolve it to bind a <see cref="LogView"/>).
    /// Call on the UI thread, typically from an <c>[STAThread] Main</c> while building the host: the feed belongs to
    /// that thread's dispatcher.
    /// </summary>
    public static ILoggingBuilder AddLogFeed(this ILoggingBuilder builder, int capacity = 2000)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Services.TryAddSingleton(LogFeed.CreateForCurrentThread(capacity));
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, LogFeedLoggerProvider>());
        return builder;
    }
}
