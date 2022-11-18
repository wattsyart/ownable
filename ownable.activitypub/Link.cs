using System.Net.Http.Headers;

namespace ownable.activitypub
{
    public sealed class Link : ObjectIdentifier, IObjectOrLink
    {
        public Uri? Href { get; set; }
        public string? Rel { get; set; }
        public MediaTypeHeaderValue? MediaType { get; set; }
        public string? Name { get; set; }
    }
}
