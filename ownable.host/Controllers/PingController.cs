using Microsoft.AspNetCore.Mvc;

namespace ownable.host.Controllers
{
    public class PingController : Controller
    {
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok();
        }
    }
}