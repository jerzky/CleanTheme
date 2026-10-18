using Microsoft.Extensions.Logging;

namespace CleanTheme.Logging;

/// <summary>
/// One line of an in-app log. <see cref="Source"/> is what <see cref="LogFeed.SourceFilter"/> matches, e.g. a child
/// process or node id; it is null for lines that come from <see cref="ILogger"/>.
/// </summary>
public sealed record LogEntry(DateTime TimestampUtc, LogLevel Level, string? Source, string Message)
{
    /// <summary>The <see cref="ILogger"/> category the line was written to, if any.</summary>
    public string? Category { get; init; }

    /// <summary>Three-letter level tag shown in front of the message.</summary>
    public string LevelTag => Level switch
    {
        LogLevel.Trace => "TRC",
        LogLevel.Debug => "DBG",
        LogLevel.Information => "INF",
        LogLevel.Warning => "WRN",
        LogLevel.Error => "ERR",
        LogLevel.Critical => "CRT",
        _ => "   ",
    };

    /// <summary>The whole line as plain text (UTC time, level, message), e.g. for copying.</summary>
    public string Text => $"{TimestampUtc:HH:mm:ss.fff}  {LevelTag}  {Message}";
}
