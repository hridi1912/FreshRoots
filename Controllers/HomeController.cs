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
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // Find this farmer
            var farmer = await _db.Farmers.FirstOrDefaultAsync(f => f.UserId == user.Id);
            if (farmer == null) return Unauthorized();

            // Products owned by this farmer
            var farmerProducts = await _db.Products
                .Where(p => p.FarmerId == farmer.FarmerId)
                .OrderBy(p => p.Id)
                .ToListAsync();

            // All order items related to this farmer
            var orderItems = await _db.OrderItems
                .Include(oi => oi.Order)
                .Where(oi => oi.FarmerId == farmer.FarmerId)
                .ToListAsync();

            // ====== CALCULATIONS ======
            // Number of unique orders involving this farmer
            int newOrders = orderItems
                .Select(oi => oi.OrderId)
                .Distinct()
                .Count();

            // Total revenue from delivered items only
            decimal totalRevenue = orderItems
                .Where(oi => oi.Status == "Delivered")   
                .Sum(oi => oi.Quantity * oi.Price);

            // Unique customers who ordered from this farmer
            int totalCustomers = orderItems
                .Select(oi => oi.Order.BuyerId)
                .Distinct()
                .Count();

            // Active products (stock > 0)
            int activeProducts = farmerProducts.Count(p => p.StockQuantity > 0);

            // Pass to View
            ViewBag.NewOrders = newOrders;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalCustomers = totalCustomers;
            ViewBag.ActiveProducts = activeProducts;

            return View(farmerProducts);
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
