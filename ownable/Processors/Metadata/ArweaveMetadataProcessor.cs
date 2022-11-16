using ownable.Models;
using Microsoft.Extensions.Logging;

namespace ownable.Processors.Metadata;

public sealed class ArweaveMetadataProcessor : HttpMetadataProcessor
{
    public ArweaveMetadataProcessor(HttpClient http, ILogger<ArweaveMetadataProcessor> logger) : base(http, logger) { }

    public override bool CanProcess(string tokenUri) => tokenUri.IsValidUriWithSchemes("ar") || tokenUri.IsValidHttpUriWithHost("arweave.net");
}