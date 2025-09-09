// Controllers/ProductsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FreshRoots.Data;
using FreshRoots.Models;
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

        public ProductsController(IWebHostEnvironment env, ApplicationDbContext db)
        {
            _env = env;
            _db = db;
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
            var items = await _db.Products.AsNoTracking().OrderByDescending(p => p.Id).ToListAsync();
            return View(items);
        }

        [Authorize(Roles = "Farmer")]
        public IActionResult Create() => View(new Product());

        [HttpPost]
        [Authorize(Roles = "Farmer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product model, IFormFile? imageFile)
        {
            if (!ModelState.IsValid) return View(model);

            if (imageFile != null && imageFile.Length > 0)
                model.ImageUrl = await SaveImage(imageFile);

            // ensure owned type isn’t null
            model.FarmerProfile ??= new FarmerProfile();

            _db.Products.Add(model);
            await _db.SaveChangesAsync();

            TempData["Msg"] = "Product created.";
            return RedirectToAction(nameof(Manage));
        }

        [Authorize(Roles = "Farmer")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null) return NotFound();
            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product is null) return NotFound();
            return View(product);
        }

        [HttpPost]
        [Authorize(Roles = "Farmer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product model, IFormFile? imageFile)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product is null) return NotFound();

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
            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
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
    }
}
