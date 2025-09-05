using Microsoft.AspNetCore.Mvc;

namespace FreshRoots.Controllers
{
    public class CartController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
