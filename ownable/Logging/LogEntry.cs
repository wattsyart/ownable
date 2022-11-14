using System.Diagnostics;
using Microsoft.Extensions.Logging;
using ownable.Logging;
using ownable.Models;
using ownable.Serialization;

namespace ownable.Logging;

[DebuggerDisplay("{LogLevel} {Message}")]
public class LogEntry
{
    public Guid Id { get; set; }
    public LogLevel LogLevel { get; set; }
    public EventId EventId { get; set; }
    public string? Message { get; set; }
    public LogException? Error { get; set; }
    public IReadOnlyList<KeyValuePair<string, string?>>? Data { get; set; }
    public DateTimeOffset Timestamp { get; set; }

    // ReSharper disable once UnusedMember.Global (Serialization)
    public LogEntry() { }

    public LogEntry(Guid id, LoggingDeserializeContext context)
    {
        Id = id;
        LogLevel = (LogLevel)context.br.ReadByte();
        EventId = new EventId(context.br.ReadInt32(), context.br.ReadNullableString());
        Message = context.br.ReadNullableString();
        Timestamp = context.br.ReadDateTimeOffset();

        if (context.br.ReadBoolean())
            Error = new LogException(context);

        if (context.br.ReadBoolean())
        {
            var count = context.br.ReadInt32();
            var list = new List<KeyValuePair<string, string?>>(count);
            for (var i = 0; i < count; i++)
            {
                var key = context.br.ReadString();
                var value = context.br.ReadNullableString();
                list.Add(new KeyValuePair<string, string?>(key, value));
            }
            Data = list;
        }
    }

    public LogEntry(Guid id, Exception? exception)
    {
        Id = id;
        if (exception != default) Error = new LogException(exception);
    }

    public void Serialize(LoggingSerializeContext context)
    {
        context.bw.Write(Id);
        context.bw.Write((byte)LogLevel);
        context.bw.Write(EventId.Id);
        context.bw.WriteNullableString(EventId.Name);
        context.bw.WriteNullableString(Message);
        context.bw.Write(Timestamp);

        if (context.bw.WriteBoolean(Error != default))
            Error?.Serialize(context);

        if (context.bw.WriteBoolean(Data != null) && Data != null)
        {
            context.bw.Write(Data.Count);
            foreach (var (key, value) in Data)
            {
                context.bw.Write(key);
                context.bw.WriteNullableString(value);
            }
        }


    }
}