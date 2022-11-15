using Microsoft.AspNetCore.Mvc;
using ownable.Data;
using ownable.Models;
using ownable.Models.Indexed;

namespace ownable.host.Controllers;

[Route("api/indices")]
public class IndexController : Controller
{
    private readonly Store _dataStore;
    private readonly ILogStore _logStore;
    private readonly ILogger<IndexController> _logger;

    public IndexController(Store dataStore, ILogStore logStore, ILogger<IndexController> logger)
    {
        _dataStore = dataStore;
        _logStore = logStore;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult GetInfo()
    {
        return Ok(new List<IndexInfo>
        {
            _dataStore.GetInfo(),
            _logStore.GetInfo()
        });
    }

    [HttpPost]
    public IActionResult BatchAppend([FromBody] List<Received> model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        foreach (var item in model)
            _dataStore.Append(item, cancellationToken);

        return Ok();
    }

    [HttpPut]
    public IActionResult BatchSave([FromBody] List<Received> model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        foreach (var item in model)
        {
            _dataStore.Save(item, cancellationToken);
        }

        return Ok();
    }
}