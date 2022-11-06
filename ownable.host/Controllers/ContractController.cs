using Microsoft.AspNetCore.Mvc;

namespace ownable.host.Controllers
{
    [Route("contracts")]
    public class ContractController : Controller
    {
        [HttpGet]
        public IActionResult GetContracts(CancellationToken cancellationToken)
        {
            var store = new Store();
            var contracts = store.GetContracts(cancellationToken);
            return Ok(contracts);
        }
    }
}
