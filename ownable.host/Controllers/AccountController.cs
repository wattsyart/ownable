using Microsoft.AspNetCore.Mvc;
using ownable.Data;
using ownable.Models.Indexed;

namespace ownable.host.Controllers;

[Route("api/accounts")]
public class AccountController : Controller
{
    private readonly Store _store;
    private readonly ILogger<AccountController> _logger;

    public AccountController(Store store, ILogger<AccountController> logger)
    {
        _store = store;
        _logger = logger;
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