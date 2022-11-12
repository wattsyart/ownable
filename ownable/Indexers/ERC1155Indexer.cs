using System.Numerics;
using Microsoft.Extensions.Logging;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using ownable.Contracts;
using ownable.Data;
using ownable.Models;
using ownable.Services;

using Contract = ownable.Models.Indexed.Contract;

namespace ownable.Indexers;

public sealed class ERC1155Indexer : ERCTokenIndexer
{
    private readonly Store _store;
    private readonly TokenService _tokenService;
    private readonly IEnumerable<IKnownContracts> _knownContracts;
    private readonly IEnumerable<IMetadataProcessor> _metadataProcessors;
    private readonly MetadataIndexer _metadataIndexer;
    private readonly ILogger<ERC721Indexer> _logger;
    private readonly ERCSpecification _specification;

    public ERC1155Indexer(Store store, TokenService tokenService, IEnumerable<IKnownContracts> knownContracts, IEnumerable<IMetadataProcessor> metadataProcessors, MetadataIndexer metadataIndexer, ILogger<ERC721Indexer> logger) : 
        base(store, tokenService, logger)
    {
        _store = store;
        _tokenService = tokenService;
        _knownContracts = knownContracts;
        _metadataProcessors = metadataProcessors;
        _metadataIndexer = metadataIndexer;
        _logger = logger;

        _specification = new ERCSpecification
        {
            standardInterface = InterfaceIds.ERC1155,
            metadataInterface = InterfaceIds.ERC1155Metadata,
            uriInterface = null,
            nameInterface = InterfaceIds.Name,
            symbolInterface = InterfaceIds.Symbol
        };
    }

    public override async Task IndexAsync(IWeb3 web3, string rootAddress, BlockParameter fromBlock, BlockParameter toBlock, IndexScope scope, CancellationToken cancellationToken)
    {
        if (scope.HasFlagFast(IndexScope.TokenTransfers))
        {
            await IndexTransfersAsync<ERC1155.TransferSingle>(web3, rootAddress, fromBlock, toBlock, scope, cancellationToken);
            await IndexTransfersAsync<ERC1155.TransferBatch>(web3, rootAddress, fromBlock, toBlock, scope, cancellationToken);
        }
    }

    protected override async Task IndexContractAddress(IWeb3 web3, string contractAddress, BigInteger tokenId, BigInteger blockNumber, IndexScope scope, CancellationToken cancellationToken)
    {
        var atBlock = blockNumber.ToBlockParameter();

        foreach (var knownContracts in _knownContracts)
        {
            if (!knownContracts.TryGetContract(contractAddress, out var contract) || contract == null)
                continue;

            _store.Append(contract, cancellationToken);
            return;
        }

        var features = await GetContractFeaturesAsync(web3, contractAddress, _specification);

        // IMPORTANT: ERC1155Metadata only specifies uri(uint256), not name or symbol!
        //            But, we will still attempt to call these, even if they fail, since most ERC165 checks
        //            in contracts fail to check for isolated name and symbol, misreporting these as unsupported when they are.
        //
        if (features.SupportsMetadata())
        {
            features |= ContractFeatures.SupportsName;
            features |= ContractFeatures.SupportsSymbol;
            features |= ContractFeatures.SupportsUri;
        }

        if (features.SupportsStandard())
        {
            var contract = new Contract
            {
                Address = contractAddress,
                Type = "ERC1155",
                Name = await _tokenService.TryGetNameAsync<ERC1155.NameFunction>(web3, contractAddress, features, atBlock),
                Symbol = await _tokenService.TryGetSymbolAsync<ERC1155.SymbolFunction>(web3, contractAddress, features, atBlock)
            };

            if (scope.HasFlagFast(IndexScope.TokenMetadata))
            {
                try
                {
                    var tokenUri = await _tokenService.TryGetTokenUriAsync<ERC1155.URIFunction>(web3, contractAddress, tokenId, features, atBlock);
                    if (tokenUri != null)
                    {
                        var foundProcessor = false;
                        JsonTokenMetadata? metadata = null;
                        foreach (var processor in _metadataProcessors)
                        {
                            if (!processor.CanProcess(tokenUri))
                                continue;

                            foundProcessor = true;
                            metadata = await processor.ProcessAsync(tokenUri, cancellationToken);

                            if (metadata == null)
                                _logger.LogWarning("Processor {ProcessorName} failed to process metadata, when it reported it was capable", processor.GetType().Name);
                        }

                        if (metadata != null)
                            await _metadataIndexer.IndexAsync(metadata, contractAddress, tokenId, blockNumber, scope, cancellationToken);

                        else if (!foundProcessor)
                            _logger.LogWarning("No metadata processor found for {Uri}", tokenUri);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Contract Address {ContractAddress} failed to fetch token URI", contractAddress);
                }
            }

            try
            {
                _store.Append(contract, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error trying to index contract with address {ContractAddress}", contractAddress);
            }
        }
    }
}