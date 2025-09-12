using FreshRoots.Data;  
using FreshRoots.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace FreshRoots.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _db;

        public HomeController(ApplicationDbContext db,
                              UserManager<ApplicationUser> userManager,
                              SignInManager<ApplicationUser> signInManager)
        {
            _db = db;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        
        public async Task<IActionResult> Index()
        {
            if (_signInManager.IsSignedIn(User))
            {
                var user = await _userManager.GetUserAsync(User);

                if (user != null && await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    // Redirect Admin to Admin Dashboard
                    return RedirectToAction("Index", "Admin");
                }
            }

            return View();
        }

        // Farmer dashboard homepage (load from DB now)
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
                .Take(6) // Limit to 6 featured products
                .ToListAsync();

            return View(products);
        }
    }
}
