using Microsoft.AspNetCore.Mvc;

namespace FreshRoots.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
