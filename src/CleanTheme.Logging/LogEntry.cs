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

    /// <summary>The category without its namespace (and generic arguments), e.g. <c>NodeSupervisor</c>.</summary>
    public string? ShortCategory => ShortenCategory(Category);

    /// <summary>Where the line came from, as shown on it: the <see cref="Source"/>, else the <see cref="ShortCategory"/>.</summary>
    public string? Origin => Source ?? ShortCategory;

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

    /// <summary>The whole line as plain text (UTC time, level, origin, message), e.g. for copying.</summary>
    public string Text => Origin is null
        ? $"{TimestampUtc:HH:mm:ss.fff}  {LevelTag}  {Message}"
        : $"{TimestampUtc:HH:mm:ss.fff}  {LevelTag}  {Origin}  {Message}";

    /// <summary>The category without its namespace (and generic arguments).</summary>
    public static string? ShortenCategory(string? category)
    {
        if (string.IsNullOrEmpty(category))
        {
            return category;
        }
        var generic = category.IndexOf('<', StringComparison.Ordinal);
        var name = generic >= 0 ? category[..generic] : category;
        return name[(name.LastIndexOf('.') + 1)..];
    }
}
