using Microsoft.AspNetCore.Mvc;

namespace SteamConsoleHelper.Web.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Ok();
        }
    }
}