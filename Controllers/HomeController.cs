using System.Diagnostics;
using FreshRoots.Models;
using Microsoft.AspNetCore.Mvc;

namespace FreshRoots.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        // Farmer dashboard homepage
        public IActionResult FarmerHome()
        {
            return View();
        }
        public IActionResult BuyerHome()
        {
            return View();
        }
    }
}
