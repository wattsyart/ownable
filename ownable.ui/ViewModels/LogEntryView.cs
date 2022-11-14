using Humanizer;
using ownable.Logging;
using ownable.Models;

namespace ownable.ui.ViewModels
{
    public sealed class LogEntryView : LogEntry
    {
        public string TimestampString => Timestamp.Humanize();

        public LogEntryView(LogEntry entry)
        {
            this.Id = entry.Id;
            this.EventId = entry.EventId;
            this.LogLevel = entry.LogLevel;
            this.Message = entry.Message;
            this.Data = entry.Data;
            this.Error = entry.Error;
            this.Timestamp = entry.Timestamp;
        }
    }
}
