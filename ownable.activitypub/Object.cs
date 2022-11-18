using System.Net.Http.Headers;

namespace ownable.activitypub;

public class Object : ObjectIdentifier, IObjectOrLink
{
    public IObjectOrLink? Attachment { get; set; }
    public IObjectOrLink[]? AttributedTo { get; set; }
    public IObjectOrLink? Audience { get; set; }
    public string? Content { get; set; }
    public IObjectOrLink? Context { get; set; }
    public string? Name { get; set; }
    public DateTimeOffset? EndTime { get; set; }
    public IObjectOrLink? Generator { get; set; }
    public IObjectOrLink? Icon { get; set; }
    public Document? Image { get; set; }
    public IObjectOrLink? InReplyTo { get; set; }
    public IObjectOrLink? Location { get; set; }
    public IObjectOrLink? Preview { get; set; }
    public DateTimeOffset? Published { get; set; }
    public ICollectionOrLink? Replies { get; set; }
    public DateTimeOffset? StartTime { get; set; }
    public string? Summary { get; set; }
    public IObjectOrLink? Tag { get; set; }
    public DateTimeOffset? Updated { get; set; }
    public IUriOrLink? Url { get; set; }
    public IObjectOrLink? To { get; set; }
    public IObjectOrLink? Bto { get; set; }
    public IObjectOrLink? Cc { get; set; }
    public IObjectOrLink? Bcc { get; set; }
    public MediaTypeHeaderValue? MediaType { get; set; }
    public TimeSpan? Duration { get; set; }
}