using System.Text.Json;
using DiscussionBoard.Models;

namespace DiscussionBoard.Tests.Starter;

public sealed class LogEntryTests
{
    [Fact]
    public void InformationLevelIsDefaultWhenOmitted()
    {
        LogEntry entry = JsonSerializer.Deserialize<LogEntry>(
            """{"@t":"2026-10-09T18:37:26Z","@m":"Started"}""")!;

        Assert.Equal("Information", entry.Level);
        Assert.Equal("Started", entry.Message);
        Assert.Equal(2026, entry.Timestamp.Year);
    }

    [Theory]
    [InlineData("Warning")]
    [InlineData("Error")]
    [InlineData("Fatal")]
    public void ExplicitLevelIsPreserved(string level)
    {
        LogEntry entry = JsonSerializer.Deserialize<LogEntry>(
            $$"""{"@t":"2026-10-09T18:37:26Z","@l":"{{level}}","@m":"Test"}""")!;

        Assert.Equal(level, entry.Level);
    }

    [Fact]
    public void ExceptionAndMessageAreRead()
    {
        LogEntry entry = JsonSerializer.Deserialize<LogEntry>(
            """{"@t":"2026-10-09T18:37:26Z","@l":"Error","@m":"Failed","@x":"Stack trace"}""")!;

        Assert.Equal("Failed", entry.Message);
        Assert.Equal("Stack trace", entry.Exception);
    }

    [Fact]
    public void MissingTimestampDoesNotInventCurrentTime()
    {
        LogEntry entry = JsonSerializer.Deserialize<LogEntry>("""{"@m":"No timestamp"}""")!;

        Assert.Equal(default, entry.Timestamp);
    }
}
