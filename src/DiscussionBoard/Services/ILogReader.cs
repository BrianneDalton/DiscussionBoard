using DiscussionBoard.Models;

namespace DiscussionBoard.Services
{
    public interface ILogReader
    {
        IEnumerable<LogEntry> Read();
    }
}
