using FreshRoots.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FreshRoots.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        // Farmer dashboard homepage
        [Authorize(Roles = "Farmer")]
        public IActionResult FarmerHome()
        {
            return View();
        }
        [Authorize(Roles = "Buyer")]
        public IActionResult BuyerHome()
        {
            return View();
        }
    }
}
