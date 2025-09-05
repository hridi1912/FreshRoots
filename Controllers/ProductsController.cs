using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FreshRoots.Models;
using FreshRoots.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FreshRoots.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public ProductsController(IWebHostEnvironment env)
        {
            _env = env;
        }

        // ----------------- PUBLIC -----------------
        [AllowAnonymous]
        public IActionResult Index(string searchString, string categoryFilter)
        {
            var products = InMemoryProductStore.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
                products = products.Where(p => p.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(categoryFilter))
                products = products.Where(p => p.Category == categoryFilter);

            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentCategory"] = categoryFilter;
            ViewBag.Categories = InMemoryProductStore.Products.Select(p => p.Category).Distinct().ToList();

            return View(products.ToList());
        }

        [AllowAnonymous]
        public IActionResult Details(int? id)
        {
            if (id is null) return NotFound();
            var product = InMemoryProductStore.Products.FirstOrDefault(p => p.Id == id);
            if (product is null) return NotFound();
            return View(product);
        }

        // ----------------- FARMER -----------------
        [Authorize(Roles = "Farmer")]
        public IActionResult Manage()
            => View(InMemoryProductStore.Products.OrderByDescending(p => p.Id).ToList());

        [Authorize(Roles = "Farmer")]
        public IActionResult Create()
            => View(new Product());

        [HttpPost]
        [Authorize(Roles = "Farmer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product model, IFormFile? imageFile)
        {
            if (!ModelState.IsValid) return View(model);

            if (imageFile != null && imageFile.Length > 0)
                model.ImageUrl = await SaveImage(imageFile);

            model.Id = InMemoryProductStore.Products.Any()
                ? InMemoryProductStore.Products.Max(p => p.Id) + 1
                : 1;

            InMemoryProductStore.Products.Add(model);

            TempData["Msg"] = "Product created.";
            return RedirectToAction(nameof(Manage));
        }

        [Authorize(Roles = "Farmer")]
        public IActionResult Edit(int? id)
        {
            if (id is null) return NotFound();
            var product = InMemoryProductStore.Products.FirstOrDefault(p => p.Id == id);
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

            var product = InMemoryProductStore.Products.FirstOrDefault(p => p.Id == id);
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

            TempData["Msg"] = "Product updated.";
            return RedirectToAction(nameof(Manage));
        }

        [HttpPost]
        [Authorize(Roles = "Farmer")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var product = InMemoryProductStore.Products.FirstOrDefault(p => p.Id == id);
            if (product is null) return NotFound();

            if (!string.IsNullOrWhiteSpace(product.ImageUrl))
            {
                var oldFsPath = ToPhysicalPath(product.ImageUrl);
                if (System.IO.File.Exists(oldFsPath))
                    System.IO.File.Delete(oldFsPath);
            }

            InMemoryProductStore.Products.Remove(product);
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
