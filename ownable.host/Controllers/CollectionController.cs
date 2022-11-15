using System.Numerics;
using Microsoft.AspNetCore.Mvc;
using ownable.Data;
using ownable.Models;
using ownable.Models.Indexed;
using ownable.Services;

namespace ownable.host.Controllers;

[Route("api/collections")]
public class CollectionController : Controller
{
    private readonly Store _store;
    private readonly MediaService _mediaService;
    private readonly ILogger<ContractController> _logger;

    public CollectionController(Store store, MediaService mediaService, ILogger<ContractController> logger)
    {
        _store = store;
        _mediaService = mediaService;
        _logger = logger;
    }

    [HttpGet("{contractAddress}")]
    public async Task<IActionResult> GetCollectionItems(string contractAddress, CancellationToken cancellationToken)
    {
        var collectionItems = new List<CollectionItem>();

        // 
        // This is hacked together, but provides a way forward for next steps in the index...
        //
        var tokenIds = GetTokenIds(contractAddress, cancellationToken);
        foreach(var tokenId in tokenIds.OrderBy(x => x))
            collectionItems.Add(await GetCollectionItemAsync(contractAddress, tokenId, cancellationToken));
        
        return Ok(collectionItems);
    }

    private IList<BigInteger> GetTokenIds(string contractAddress, CancellationToken cancellationToken)
    {
        var allTraitsForEveryTokenInContract = KeyBuilder.KeyLookup(typeof(Trait), nameof(Trait.ContractAddress), contractAddress);
        var traits = _store.FindByKey<Trait>(allTraitsForEveryTokenInContract, cancellationToken);
        return traits.Select(x => x.TokenId).Distinct().ToList();
    }

    [HttpGet("{contractAddress}/{tokenId}")]
    public async Task<IActionResult> GetCollectionItem(string contractAddress, BigInteger tokenId, CancellationToken cancellationToken)
    {
        var collectionItem = await GetCollectionItemAsync(contractAddress, tokenId, cancellationToken);

        return Ok(collectionItem);
    }

    private async Task<CollectionItem> GetCollectionItemAsync(string contractAddress, BigInteger tokenId, CancellationToken cancellationToken)
    {
        var traits = GetTokenTraits(contractAddress, tokenId, cancellationToken);
        var metadata = GetTokenMetadata(contractAddress, tokenId, cancellationToken);

        var blockNumber = traits.First().BlockNumber;
        var extension = ".gif";
        var mediaType = "image/gif";
        var media = await _mediaService.GetMediaAsync(contractAddress, tokenId, blockNumber, extension, cancellationToken);

        var collectionItem = new CollectionItem
        {
            Traits = traits,
            Media = $"data:{mediaType};base64,{media}",
            Name = metadata.Name,
            Description = metadata.Description,
            ExternalUrl = metadata.ExternalUrl,
            TokenId = metadata.TokenId
        };

        return collectionItem;
    }

    private Metadata GetTokenMetadata(string contractAddress, BigInteger tokenId, CancellationToken cancellationToken)
    {
        var findByContractAddressAndTokenId = KeyBuilder.KeyLookup(typeof(Metadata), new[]
        {
            nameof(Metadata.ContractAddress),
            nameof(Metadata.TokenId)
        }, new[]
        {
            contractAddress,
            tokenId.ToString()
        });
        var metadata = _store.FindByKey<Metadata>(findByContractAddressAndTokenId, cancellationToken);
        return metadata.Single();
    }

    private IList<Trait> GetTokenTraits(string contractAddress, BigInteger tokenId, CancellationToken cancellationToken)
    {
        var findByContractAddressAndTokenId = KeyBuilder.KeyLookup(typeof(Trait), new[]
        {
            nameof(Trait.ContractAddress),
            nameof(Trait.TokenId)
        }, new[]
        {
            contractAddress,
            tokenId.ToString()
        });
        var traits = _store.FindByKey<Trait>(findByContractAddressAndTokenId, cancellationToken);
        return traits.ToList();
    }
}