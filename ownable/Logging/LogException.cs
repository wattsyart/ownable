using ownable.Models;
using ownable.Logging;
using ownable.Serialization;

namespace ownable.Logging;

public sealed class LogException
{
    // ReSharper disable once UnusedMember.Global (needed for deserialization)
    public LogException() { }

    public LogException(Exception exception)
    {
        Message = exception.Message;
        StackTrace = exception.StackTrace;
        HelpLink = exception.HelpLink;
        Source = exception.Source;
    }

    public LogException(LoggingDeserializeContext context)
    {
        Message = context.br.ReadNullableString();
        StackTrace = context.br.ReadNullableString();
        HelpLink = context.br.ReadNullableString();
        Source = context.br.ReadNullableString();
    }

    public string? Message { get; set; }
    public string? StackTrace { get; set; }
    public string? HelpLink { get; set; }
    public string? Source { get; set; }

    public void Serialize(LoggingSerializeContext context)
    {
        context.bw.WriteNullableString(Message);
        context.bw.WriteNullableString(StackTrace);
        context.bw.WriteNullableString(HelpLink);
        context.bw.WriteNullableString(Source);
    }
}