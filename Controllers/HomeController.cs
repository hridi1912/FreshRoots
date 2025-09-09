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
        public async Task<IActionResult> BuyerHome()
        {
            // Get featured products (you can customize this query)
            var products = await _db.Products
                .Include(p => p.FarmerProfile) // Include farmer profile if needed
                .Where(p => p.StockQuantity > 0) // Only show products in stock
                .OrderByDescending(p => p.HarvestDate) // Show newest first
                .Take(3) // Limit to 6 featured products
                .ToListAsync();

            return View(products);
        }

    }
}
