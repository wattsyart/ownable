using Microsoft.AspNetCore.Mvc;
using ownable.Data;
using ownable.Models.Indexed;

namespace ownable.host.Controllers;

[Route("api/contracts")]
public class ContractController : Controller
{
    private readonly Store _store;
    private readonly ILogger<ContractController> _logger;

    public ContractController(Store store, ILogger<ContractController> logger)
    {
        _store = store;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult GetContracts(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Entries: {Count}", _store.GetEntriesCount(cancellationToken));
        var model = _store.Get<Contract>(cancellationToken);
        return Ok(model);
    }
}