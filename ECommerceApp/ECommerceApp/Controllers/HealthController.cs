using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Controllers
{
    [ApiController] // Indicates that this controller responds to web API requests
    [Route("api/health")]
    public class HealthController : Controller
    {
        [HttpGet]
        public IActionResult Get()
        {
            // You can add more complex checks here, e.g., database connectivity, external service status
            return Ok("API is healthy and running!");
        }
    }
}
