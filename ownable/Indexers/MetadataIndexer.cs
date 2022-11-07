using System.Numerics;
using Microsoft.Extensions.Logging;
using ownable.Indexers.Handlers;
using ownable.Models;

namespace ownable.Indexers
{
    internal sealed class MetadataIndexer
    {
        private readonly IEnumerable<IMetadataImageProcessor> _imageProcessors;
        private readonly IEnumerable<IMetadataImageHandler> _imageHandlers;
        private readonly ILogger<MetadataIndexer> _logger;

        public MetadataIndexer(IEnumerable<IMetadataImageProcessor> imageProcessors, IEnumerable<IMetadataImageHandler> imageHandlers, ILogger<MetadataIndexer> logger)
        {
            _imageProcessors = imageProcessors;
            _imageHandlers = imageHandlers;
            _logger = logger;
        }

        public async Task IndexAsync(JsonTokenMetadata metadata, string contractAddress, BigInteger tokenId, CancellationToken cancellationToken)
        {
            foreach (var processor in _imageProcessors)
            {
                if (!processor.CanProcess(metadata))
                    continue;

                var processorName = processor.GetType().Name;
                _logger.LogInformation("Processing metadata with {ProcessorName}", processorName);

                var (stream, extension) = await processor.ProcessAsync(metadata, cancellationToken);

                if (stream == null)
                {
                    _logger.LogWarning("Processor {ProcessorName} failed to process metadata, when it reported it was capable", processorName);
                }
                else
                {
                    foreach (var handler in _imageHandlers)
                    {
                        if (await handler.HandleAsync(stream, contractAddress, tokenId, extension, cancellationToken))
                            break;
                    }
                }
            }
        }
    }
}
