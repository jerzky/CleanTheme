using CleanTheme.Wpf.Tests;
using Microsoft.Extensions.Logging;

namespace CleanTheme.Logging.Tests;

public sealed class LogFeedTests
{
    [Fact]
    public void Lines_beyond_capacity_drop_the_oldest() => Sta.Run(() =>
    {
        var feed = LogFeed.CreateForCurrentThread(capacity: 3);

        for (var i = 0; i < 5; i++)
        {
            feed.Add(Line(i.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        }

        Assert.Equal(["2", "3", "4"], feed.Visible.Select(e => e.Message));
    });

    [Fact]
    public void Source_filter_shows_only_that_source_and_keeps_the_rest() => Sta.Run(() =>
    {
        var feed = LogFeed.CreateForCurrentThread();
        feed.Add(Line("server"));
        feed.Add(Line("a1", source: "A"));
        feed.Add(Line("b1", source: "B"));

        feed.SourceFilter = "A";
        Assert.Equal(["a1"], feed.Visible.Select(e => e.Message));

        feed.SourceFilter = null;
        Assert.Equal(["server", "a1", "b1"], feed.Visible.Select(e => e.Message));
    });

    [Fact]
    public void Minimum_level_hides_quieter_lines() => Sta.Run(() =>
    {
        var feed = LogFeed.CreateForCurrentThread();
        feed.Add(Line("debug", LogLevel.Debug));
        feed.Add(Line("info"));
        feed.Add(Line("error", LogLevel.Error));

        feed.MinimumLevel = LogLevel.Information;
        Assert.Equal(["info", "error"], feed.Visible.Select(e => e.Message));

        feed.Add(Line("trace", LogLevel.Trace));
        Assert.Equal(["info", "error"], feed.Visible.Select(e => e.Message));
    });

    [Fact]
    public void Add_from_another_thread_throws() => Sta.Run(() =>
    {
        var feed = LogFeed.CreateForCurrentThread();
        Exception? error = null;

        var thread = new Thread(() => error = Record.Exception(() => feed.Add(Line("x"))));
        thread.Start();
        thread.Join();

        Assert.IsType<InvalidOperationException>(error);
    });

    [Fact]
    public void Posts_from_many_threads_all_arrive_in_per_thread_order() => Sta.Run(() =>
    {
        const int threads = 8;
        const int perThread = 500;
        var feed = LogFeed.CreateForCurrentThread(capacity: threads * perThread);
        using var start = new ManualResetEventSlim();

        var writers = Enumerable.Range(0, threads).Select(t => new Thread(() =>
        {
            start.Wait();
            for (var i = 0; i < perThread; i++)
            {
                feed.Post(new LogEntry(DateTime.UtcNow, LogLevel.Information, $"T{t}", $"{i}"));
            }
        })).ToArray();
        foreach (var writer in writers)
        {
            writer.Start();
        }
        start.Set();
        foreach (var writer in writers)
        {
            writer.Join();
        }
        Sta.DoEvents();

        Assert.Equal(threads * perThread, feed.Visible.Count);
        foreach (var group in feed.Visible.GroupBy(e => e.Source))
        {
            Assert.Equal(Enumerable.Range(0, perThread), group.Select(e => int.Parse(e.Message, System.Globalization.CultureInfo.InvariantCulture)));
        }
    });

    [Fact]
    public void Clear_also_drops_queued_posts() => Sta.Run(() =>
    {
        var feed = LogFeed.CreateForCurrentThread();
        feed.Add(Line("kept until clear"));
        feed.Post(Line("queued"));

        feed.Clear();
        Sta.DoEvents();

        Assert.Empty(feed.Visible);
    });

    [Fact]
    public void LevelTag_and_Text_format_the_line()
    {
        var entry = new LogEntry(new DateTime(2026, 10, 18, 9, 5, 7, 42, DateTimeKind.Utc), LogLevel.Warning, null, "Slow tick");

        Assert.Equal("WRN", entry.LevelTag);
        Assert.Equal("09:05:07.042  WRN  Slow tick", entry.Text);
    }

    private static LogEntry Line(string message, LogLevel level = LogLevel.Information, string? source = null) =>
        new(DateTime.UtcNow, level, source, message);
}
