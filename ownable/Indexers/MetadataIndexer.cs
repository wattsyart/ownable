using System.Numerics;
using Microsoft.Extensions.Logging;
using ownable.Data;
using ownable.Models;
using ownable.Models.Indexed;

namespace ownable.Indexers;

public sealed class MetadataIndexer
{
    private readonly Store _store;

    private readonly IEnumerable<IMetadataImageProcessor> _imageProcessors;
    private readonly IEnumerable<IMetadataImageHandler> _imageHandlers;
    private readonly ILogger<MetadataIndexer> _logger;

    public MetadataIndexer(Store store, IEnumerable<IMetadataImageProcessor> imageProcessors, IEnumerable<IMetadataImageHandler> imageHandlers, ILogger<MetadataIndexer> logger)
    {
        _store = store;
        _imageProcessors = imageProcessors;
        _imageHandlers = imageHandlers;
        _logger = logger;
    }

    public async Task IndexAsync(JsonTokenMetadata metadata, string contractAddress, BigInteger tokenId, BigInteger blockNumber, IndexScope scope, CancellationToken cancellationToken)
    {
        if (scope.HasFlagFast(IndexScope.TokenMetadata))
        {
            _store.Save(new Metadata
            {
                Name = metadata.Name,
                Description = metadata.Description,
                ExternalUrl = metadata.ExternalUrl,
                ContractAddress = contractAddress,
                TokenId = tokenId,
                BlockNumber = blockNumber
            }, cancellationToken);
        }

        if (scope.HasFlagFast(IndexScope.TokenMetadataAttributes) && metadata.Attributes != null)
        {
            foreach (var attribute in metadata.Attributes)
            {
                _store.Save(new Trait
                {
                    Type = attribute.TraitType,
                    Value = attribute.Value,
                    ContractAddress = contractAddress,
                    TokenId = tokenId,
                    BlockNumber = blockNumber
                }, cancellationToken);
            }
        }

        foreach (var processor in _imageProcessors)
        {
            if (!processor.CanProcess(metadata))
                continue;

            var processorName = processor.GetType().Name;
            _logger.LogInformation("Processing metadata images with {ProcessorName}", processorName);

            var (stream, media) = await processor.ProcessAsync(metadata, cancellationToken);
            
            if (scope.HasFlagFast(IndexScope.TokenMedia) && media != null)
            {
                _store.Save(media, cancellationToken);
            }

            if (stream == null)
            {
                _logger.LogWarning("Processor {ProcessorName} failed to process metadata, when it reported it was capable", processorName);
            }
            else
            {
                foreach (var handler in _imageHandlers)
                {
                    if (await handler.HandleAsync(stream, media, contractAddress, tokenId, blockNumber, scope, cancellationToken))
                        break;
                }
            }
        }
    }
}