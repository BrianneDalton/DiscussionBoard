using DiscussionBoard.Models;
using DiscussionBoard.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging.Abstractions;

namespace DiscussionBoard.Tests.Starter;

public sealed class LogsPageTests
{
    [Fact]
    public void EmptyLevelShowsAllEntries()
    {
        DiscussionBoard.Pages.Logs page = CreatePage("", SampleEntries());

        Assert.IsType<PageResult>(page.OnGet());
        Assert.Equal(5, page.Entries.Count);
    }

    [Fact]
    public void FilteringIsCaseInsensitiveAndRemovesConsecutiveNonmatches()
    {
        DiscussionBoard.Pages.Logs page = CreatePage("wArNiNg", SampleEntries());

        Assert.IsType<PageResult>(page.OnGet());
        Assert.Single(page.Entries);
        Assert.Equal("Warning", page.Entries[0].Level);
    }

    [Fact]
    public void NullLogLevelDoesNotCrashFiltering()
    {
        DiscussionBoard.Pages.Logs page = CreatePage("Error", [
            new LogEntry { Level = null, Message = "Null" },
            new LogEntry { Level = "Error", Message = "Keep" }
        ]);

        Assert.IsType<PageResult>(page.OnGet());
        Assert.Single(page.Entries);
        Assert.Equal("Keep", page.Entries[0].Message);
    }

    [Fact]
    public void ReaderFailureRedirectsToErrorPage()
    {
        DiscussionBoard.Pages.Logs page = new(
            NullLogger<DiscussionBoard.Pages.Logs>.Instance,
            new FakeLogReader(throwOnRead: true));

        RedirectToPageResult redirect = Assert.IsType<RedirectToPageResult>(page.OnGet());
        Assert.Equal("/Error", redirect.PageName);
    }

    private static DiscussionBoard.Pages.Logs CreatePage(string level, IEnumerable<LogEntry> entries)
    {
        return new DiscussionBoard.Pages.Logs(
            NullLogger<DiscussionBoard.Pages.Logs>.Instance,
            new FakeLogReader(entries))
        {
            Level = level
        };
    }

    private static LogEntry[] SampleEntries() => [
        new() { Level = "Information", Message = "One" },
        new() { Level = "Error", Message = "Two" },
        new() { Level = "Error", Message = "Three" },
        new() { Level = "Warning", Message = "Four" },
        new() { Level = "Information", Message = "Five" }
    ];

    private sealed class FakeLogReader(IEnumerable<LogEntry>? entries = null, bool throwOnRead = false) : ILogReader
    {
        public IEnumerable<LogEntry> Read()
        {
            if (throwOnRead)
            {
                throw new IOException("Simulated log read failure");
            }

            return entries ?? [];
        }
    }
}
