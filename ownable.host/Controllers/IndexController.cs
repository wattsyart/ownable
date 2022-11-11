using Microsoft.AspNetCore.Mvc;
using ownable.Data;
using ownable.Models.Indexed;

namespace ownable.host.Controllers;

[Route("indices")]
public class IndexController : Controller
{
    private readonly Store _store;
    private readonly ILogger<IndexController> _logger;

    public IndexController(Store store, ILogger<IndexController> logger)
    {
        _store = store;
        _logger = logger;
    }

    [HttpPost]
    public IActionResult BatchAppend([FromBody] List<Received> model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        foreach (var item in model)
        {
            _store.Append(item, cancellationToken);
        }


        return Ok();
    }

    [HttpPut]
    public IActionResult BatchSave([FromBody] List<Received> model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        foreach (var item in model)
        {
            _store.Save(item, cancellationToken);
        }

        return Ok();
    }
}