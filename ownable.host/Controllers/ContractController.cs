using Microsoft.AspNetCore.Mvc;
using ownable.Models;

namespace ownable.host.Controllers
{
    [Route("contracts")]
    public class ContractController : Controller
    {
        [HttpGet]
        public IActionResult GetContracts(CancellationToken cancellationToken)
        {
            var store = new Store();
            var contracts = store.Get<Contract>(cancellationToken);
            return Ok(contracts);
        }
    }
}
