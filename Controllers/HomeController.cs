using FreshRoots.Models;

using Microsoft.AspNetCore.Authorization;

//using FreshRoots.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FreshRoots.Data;  // Make sure you include this to use the ApplicationDbContext
using System.Linq;

namespace FreshRoots.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;

        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }


        // Farmer dashboard homepage (load from DB now)
        //public async Task<IActionResult> FarmerHome()

        // Farmer dashboard homepage
        [Authorize(Roles = "Farmer")]
        public async Task<IActionResult> FarmerHome()

        {
            var products = await _db.Products.OrderBy(p => p.Id).ToListAsync();

            return View(products);
        }

        [Authorize(Roles = "Buyer")]

        public IActionResult BuyerHome()
        {
            return View();
        }
    }
}
