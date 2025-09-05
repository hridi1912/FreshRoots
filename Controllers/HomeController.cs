using FreshRoots.Models;
using FreshRoots.Services;
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
        public IActionResult FarmerHome()
        {
            var products = InMemoryProductStore.Products
            .OrderBy(p => p.Id)
            .ToList();

            return View(products);
        }
        public IActionResult BuyerHome()
        {
            return View();
        }
    }
}
