using Microsoft.Extensions.Logging;
using ownable.Models;

namespace ownable.Processors.Images;

internal sealed class ArweaveImageProcessor : HttpImageProcessor
{
    public ArweaveImageProcessor(HttpClient http, ILogger<IMetadataImageProcessor> logger) : base(http, logger) { }

    public override bool CanProcess(JsonTokenMetadata metadata) => metadata.HasUriImageWithSchemes("ar") || metadata.HasHttpImageWithHost("arweave.net");
}