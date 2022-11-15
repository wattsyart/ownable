using Microsoft.AspNetCore.Mvc;
using ownable.Models;

namespace ownable.host.Controllers
{
    [Route("api/logs")]
    public class LogController : Controller
    {
        private readonly ILogStore _store;

        public LogController(ILogStore store)
        {
            _store = store;
        }

        [HttpGet]
        public IActionResult GetAll(CancellationToken cancellationToken)
        {
            var logs = _store.Get(cancellationToken);
            return Ok(logs.OrderByDescending(x => x.Timestamp));
        }
    }
}
