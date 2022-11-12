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

public sealed class ERC721Indexer : ERCTokenIndexer
{
    private readonly Store _store;
    private readonly TokenService _tokenService;
    private readonly IEnumerable<IKnownContracts> _knownContracts;
    private readonly IEnumerable<IMetadataProcessor> _metadataProcessors;
    private readonly MetadataIndexer _metadataIndexer;
    private readonly ILogger<ERC721Indexer> _logger;

    public ERC721Indexer(Store store, TokenService tokenService, IEnumerable<IKnownContracts> knownContracts, IEnumerable<IMetadataProcessor> metadataProcessors, MetadataIndexer metadataIndexer,  ILogger<ERC721Indexer> logger) :
        base(store, tokenService, logger)
    {
        _store = store;
        _tokenService = tokenService;
        _knownContracts = knownContracts;
        _metadataProcessors = metadataProcessors;
        _metadataIndexer = metadataIndexer;
        _logger = logger;
    }

    public override async Task IndexAccountAsync(IWeb3 web3, string account, BlockParameter fromBlock, BlockParameter toBlock, IndexScope scope, CancellationToken cancellationToken)
    {
        await IndexTransfersAsync<ERC721.Transfer>(web3, account, fromBlock, toBlock, scope, cancellationToken);
    }

    protected override async Task IndexTokenContractAsync(IWeb3 web3, string contractAddress, BigInteger tokenId, BigInteger blockNumber, IndexScope scope, CancellationToken cancellationToken)
    {
        var atBlock = blockNumber.ToBlockParameter();

        foreach (var knownContracts in _knownContracts)
        {
            if (!knownContracts.TryGetContract(contractAddress, out var contract) || contract == null)
                continue;
            contract.BlockNumber = blockNumber;
            _store.Append(new Contract
            {
                Address = contract.Address,
                BlockNumber = contract.BlockNumber,
                Name = contract.Name,
                Symbol = contract.Symbol,
                Type = contract.Type
            }, cancellationToken);
            return;
        }

        var features = await _tokenService.GetContractFeaturesAsync(web3, contractAddress, ERCSpecification.ERC721);

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
                Type = "ERC721",
                BlockNumber = blockNumber,
                Name = await _tokenService.TryGetNameAsync<ERC721.NameFunction>(web3, contractAddress, features, atBlock),
                Symbol = await _tokenService.TryGetSymbolAsync<ERC721.SymbolFunction>(web3, contractAddress, features, atBlock)
            };

            try
            {
                var tokenUri = await _tokenService.TryGetTokenUriAsync<ERC721.TokenURIFunction>(web3, contractAddress, tokenId, features, atBlock);
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