using Microsoft.AspNetCore.Mvc;
using ownable.Models;

namespace ownable.host.Controllers;

[Route("contracts")]
public class ContractController : Controller
{
    private readonly Store _store;

    public ContractController(Store store)
    {
        _store = store;
    }

    [HttpGet]
    public IActionResult GetContracts(CancellationToken cancellationToken)
    {
        return Ok(_store.Get<Contract>(cancellationToken));
    }
}