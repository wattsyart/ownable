using System.Numerics;
using Microsoft.AspNetCore.Mvc;
using ownable.Data;
using ownable.Models.Indexed;

namespace ownable.host.Controllers;

[Route("api/collections")]
public class CollectionController : Controller
{
    private readonly Store _store;
    private readonly ILogger<ContractController> _logger;

    public CollectionController(Store store, ILogger<ContractController> logger)
    {
        _store = store;
        _logger = logger;
    }

    [HttpGet("{contractAddress}/{tokenId}")]
    public IActionResult GetToken(string contractAddress, BigInteger tokenId, CancellationToken cancellationToken)
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
        return Ok(traits);
    }
}