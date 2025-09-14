// Controllers/ProductsController.cs
using FreshRoots.Data;
using FreshRoots.Models;
using FreshRoots.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FreshRoots.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FreshRoots.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductsController(IWebHostEnvironment env, ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _env = env;
            _db = db;
            _userManager = userManager;
        }


        [AllowAnonymous]
        public async Task<IActionResult> Index(string? searchString, string? categoryFilter)
        {
            var query = _db.Products.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
                query = query.Where(p => EF.Functions.Like(p.Name, $"%{searchString}%"));

            if (!string.IsNullOrWhiteSpace(categoryFilter))
                query = query.Where(p => p.Category == categoryFilter);

            var list = await query
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentCategory"] = categoryFilter;

            var categories = await _db.Products
                .AsNoTracking()
                .Select(p => p.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            ViewBag.Categories = categories;

            return View(list);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id is null) return NotFound();
            var product = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            if (product is null) return NotFound();
            return View(product);
        }

        // ----------------- FARMER -----------------
        [Authorize(Roles = "Farmer")]
        public async Task<IActionResult> Manage()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // ✅ Only return products belonging to the logged-in farmer
            var products = await _db.Products
                .Include(p => p.FarmerProfile)
                .Where(p => p.FarmerId == user.Id)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            return View(products);
        }

        [Authorize(Roles = "Farmer")]
        public IActionResult Create() => View(new Product());

        [HttpPost]
        [Authorize(Roles = "Farmer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product model, IFormFile? imageFile)
        {

            if (!ModelState.IsValid) return View(model);

            // Assign the current logged-in farmer
            var user = await _userManager.GetUserAsync(User);
           
            model.FarmerId = user.Id;
           

            if (imageFile != null && imageFile.Length > 0)
                model.ImageUrl = await SaveImage(imageFile);

            model.FarmerProfile ??= new FarmerProfile();

            _db.Products.Add(model);
            await _db.SaveChangesAsync();

            TempData["Msg"] = "Product created.";
            return RedirectToAction(nameof(Manage));
        }

        [Authorize(Roles = "Farmer")]
        public async Task<IActionResult> Edit(int? id)
        {
            //if (id is null) return NotFound();
            //var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);

            //if (product is null) return NotFound();
            //return View(product);
            if (id is null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id && p.FarmerId == user.Id);

            if (product is null) return NotFound(); // not found or not owned
            return View(product);
        }

        [HttpPost]
        [Authorize(Roles = "Farmer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product model, IFormFile? imageFile)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);
            var user = await _userManager.GetUserAsync(User);
            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id && p.FarmerId == user.Id);

            //var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product is null) return NotFound();

            if (string.IsNullOrWhiteSpace(product.FarmerId))
            {
               // var user = await _userManager.GetUserAsync(User);
                product.FarmerId = user?.Id;
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                if (!string.IsNullOrWhiteSpace(product.ImageUrl))
                {
                    var oldFsPath = ToPhysicalPath(product.ImageUrl);
                    if (System.IO.File.Exists(oldFsPath))
                        System.IO.File.Delete(oldFsPath);
                }
                product.ImageUrl = await SaveImage(imageFile);
            }

            product.Name = model.Name;
            product.Description = model.Description;
            product.Price = model.Price;
            product.StockQuantity = model.StockQuantity;
            product.Category = model.Category;
            product.HarvestDate = model.HarvestDate;

            product.FarmerProfile ??= new FarmerProfile();
            product.FarmerProfile.FarmName = model.FarmerProfile?.FarmName;
            product.FarmerProfile.Certification = model.FarmerProfile?.Certification;

            await _db.SaveChangesAsync();

            TempData["Msg"] = "Product updated.";
            return RedirectToAction(nameof(Manage));
        }

        [HttpPost]
        [Authorize(Roles = "Farmer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {

            var user = await _userManager.GetUserAsync(User);
            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id && p.FarmerId == user.Id);

            if (product is null) return NotFound();
            //var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product is null) return NotFound();

            if (!string.IsNullOrWhiteSpace(product.ImageUrl))
            {
                var oldFsPath = ToPhysicalPath(product.ImageUrl);
                if (System.IO.File.Exists(oldFsPath))
                    System.IO.File.Delete(oldFsPath);
            }

            _db.Products.Remove(product);
            await _db.SaveChangesAsync();

            TempData["Msg"] = "Product deleted.";
            return RedirectToAction(nameof(Manage));
        }

        // ----------------- Helpers -----------------
        private async Task<string> SaveImage(IFormFile imageFile)
        {
            var ext = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            if (!allowed.Contains(ext))
                throw new Exception("Only JPG, JPEG, PNG, WEBP or GIF files are allowed.");

            var uploadsRoot = Path.Combine(_env.WebRootPath!, "images", "products");
            Directory.CreateDirectory(uploadsRoot);

            var fileName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(uploadsRoot, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
                await imageFile.CopyToAsync(stream);

            return $"/images/products/{fileName}";
        }

        private string ToPhysicalPath(string relativeWebPath)
        {
            var path = relativeWebPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(_env.WebRootPath ?? string.Empty, path);
        }

        
        [Authorize(Roles = "Farmer")]
        public async Task<IActionResult> FarmerDashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // Get only the farmer's products
            var products = await _db.Products
                .Where(p => p.FarmerId == user.Id)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            // Calculate real stats and store in ViewBag
            ViewBag.NewOrders = await _db.OrderItems
                .Where(oi => oi.FarmerId == user.Id && oi.Status == "Pending")
                .CountAsync();

            ViewBag.ActiveProducts = await _db.Products
                .Where(p => p.FarmerId == user.Id && p.StockQuantity > 0)
                .CountAsync();

            ViewBag.TotalCustomers = await _db.OrderItems
                .Where(oi => oi.FarmerId == user.Id)
                .Select(oi => oi.Order.BuyerId)
                .Distinct()
                .CountAsync();

            ViewBag.TotalRevenue = await _db.OrderItems
                .Where(oi => oi.FarmerId == user.Id && oi.Status == "Delivered")
                .SumAsync(oi => oi.Price * oi.Quantity);

            return View(products);
        }

        [HttpGet]
        [Authorize(Roles = "Farmer")]
        public async Task<IActionResult> GetStats()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var stats = new
            {
                newOrders = await _db.OrderItems
                    .Where(oi => oi.FarmerId == user.Id && oi.Status == "Pending")
                    .CountAsync(),
                activeProducts = await _db.Products
                    .Where(p => p.FarmerId == user.Id && p.StockQuantity > 0)
                    .CountAsync(),
                totalCustomers = await _db.OrderItems
                    .Where(oi => oi.FarmerId == user.Id)
                    .Select(oi => oi.Order.BuyerId)
                    .Distinct()
                    .CountAsync(),
                totalRevenue = await _db.OrderItems
                    .Where(oi => oi.FarmerId == user.Id && oi.Status == "Delivered")
                    .SumAsync(oi => oi.Price * oi.Quantity)
            };

            return Ok(stats);
        }
        public async Task<IActionResult> TopProductAnalytics()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // Get top 4 best-selling products for this farmer
          var topProducts = await _db.OrderItems
         .Where(oi => oi.FarmerId == user.Id && oi.Status == "Delivered")
         .Include(oi => oi.Product) // Important: Include the Product
         .GroupBy(oi => oi.ProductId)
         .Select(g => new TopProductViewModel
         {
             Product = g.First().Product,
             TotalSold = g.Sum(oi => oi.Quantity),
             TotalRevenue = g.Sum(oi => oi.Price * oi.Quantity)
         })
         .OrderByDescending(x => x.TotalSold)
         .Take(4)
         .ToListAsync();

            return PartialView("_TopProductAnalytics", topProducts);
        }
        //BuyerDashboardStats
        [Authorize(Roles = "Buyer")]
        public async Task<IActionResult> BuyerDashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // Active Orders (Pending, Processing, Shipped)
            ViewBag.ActiveOrders = await _db.Orders
                .Where(o => o.BuyerId == user.Id &&
                           (o.Status == "Pending" || o.Status == "Processing" || o.Status == "Shipped"))
                .CountAsync();

            // Total Spent (All delivered orders)
            ViewBag.TotalSpent = await _db.Orders
                .Where(o => o.BuyerId == user.Id && o.Status == "Delivered")
                .SumAsync(o => o.TotalAmount);

            // Total Deliveries
            ViewBag.TotalDeliveries = await _db.Orders
                .Where(o => o.BuyerId == user.Id && o.Status == "Delivered")
                .CountAsync();

            // Carbon Footprint Saved (Calculate based on orders)
            var deliveredOrders = await _db.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.BuyerId == user.Id && o.Status == "Delivered")
                .ToListAsync();

            decimal totalCarbonSaved = 0;

            foreach (var order in deliveredOrders)
            {
                foreach (var item in order.OrderItems)
                {
                    // Estimate carbon savings based on product type and distance
                    // Local produce saves ~2kg CO2 per kg compared to conventional supply chain
                    decimal carbonPerKg = 2.0m;
                    totalCarbonSaved += (item.Quantity * carbonPerKg);
                }
            }

            ViewBag.CarbonSaved = totalCarbonSaved;

            // Get products for the view
            var products = await _db.Products
                .Include(p => p.FarmerProfile)
                .OrderByDescending(p => p.Id)
                .Take(4)
                .ToListAsync();

            return View("BuyerHome", products);
        }
        [Authorize(Roles = "Buyer")]
        [HttpGet]
        public async Task<IActionResult> GetBuyerDashboardStats()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // Active Orders (Pending, Processing, Shipped)
            var activeOrders = await _db.Orders
                .Where(o => o.BuyerId == user.Id &&
                           (o.Status == "Pending" || o.Status == "Processing" || o.Status == "Shipped"))
                .CountAsync();

            // Total Spent (All delivered orders)
            var totalSpent = await _db.Orders
                .Where(o => o.BuyerId == user.Id && o.Status == "Delivered")
                .SumAsync(o => o.TotalAmount);

            // Total Deliveries
            var totalDeliveries = await _db.Orders
                .Where(o => o.BuyerId == user.Id && o.Status == "Delivered")
                .CountAsync();

            // Carbon Footprint Saved (Calculate based on orders)
            var deliveredOrders = await _db.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.BuyerId == user.Id && o.Status == "Delivered")
                .ToListAsync();

            decimal totalCarbonSaved = 0;

            foreach (var order in deliveredOrders)
            {
                foreach (var item in order.OrderItems)
                {
                    // Estimate carbon savings based on product type and distance
                    // Local produce saves ~2kg CO2 per kg compared to conventional supply chain
                    decimal carbonPerKg = 2.0m;
                    totalCarbonSaved += (item.Quantity * carbonPerKg);
                }
            }

            return Json(new
            {
                activeOrders,
                totalSpent,
                totalDeliveries,
                carbonSaved = totalCarbonSaved
            });
        }
    }
}
