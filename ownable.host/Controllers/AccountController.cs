using Microsoft.AspNetCore.Mvc;
using ownable.Data;
using ownable.Models.Indexed;

namespace ownable.host.Controllers;

[Route("accounts")]
public class AccountController : Controller
{
    private readonly Store _store;

    public AccountController(Store store)
    {
        _store = store;
    }

    [HttpGet("sent")]
    public IActionResult GetSent(CancellationToken cancellationToken)
    {
        return Ok(_store.Get<Sent>(cancellationToken));
    }

    [HttpGet("received")]
    public IActionResult GetReceived(CancellationToken cancellationToken)
    {
        return Ok(_store.Get<Received>(cancellationToken));
    }
}