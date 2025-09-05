using Microsoft.AspNetCore.Mvc;
using FreshRoots.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FreshRoots.Controllers
{
    public class ProductsController : Controller
    {
        // Mock data - Replace this with database context later
        private static List<Product> _products = new List<Product>
        {
            new Product
            {
                Id = 1,
                Name = "Organic Tomatoes",
                Description = "Fresh organic tomatoes, harvested yesterday",
                Price = 40,
                StockQuantity = 20,
                ImageUrl = "https://images.unsplash.com/photo-1561136594-7f68413baa99?fm=jpg&q=60&w=3000&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxzZWFyY2h8MTJ8fHRvbWF0b3xlbnwwfHwwfHx8MA%3D%3D",
                Category = "Vegetables",
                HarvestDate = System.DateTime.Now.AddDays(-1),
                FarmerProfile = new FarmerProfile { FarmName = "Green Valley Farm", Certification = "Organic" }
            },
            new Product
            {
                Id = 2,
                Name = "Fresh Apples",
                Description = "Sweet and crunchy apples from our orchard",
                Price = 240,
                StockQuantity = 15,
                ImageUrl = "https://images.unsplash.com/photo-1576179636333-f13b174b3c9a?fm=jpg&q=60&w=3000&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxzZWFyY2h8MTR8fGFwcGxlJTIwb3JjaGFyZHxlbnwwfHwwfHx8MA%3D%3D",
                Category = "Fruits",
                HarvestDate = System.DateTime.Now.AddDays(-2),
                FarmerProfile = new FarmerProfile { FarmName = "Sunny Orchard", Certification = "Local" }
            },
            new Product
            {
                Id = 3,
                Name = "Free-Range Eggs",
                Description = "Farm fresh eggs from free-range chickens",
                Price = 140,
                StockQuantity = 12,
                ImageUrl = "https://images.unsplash.com/photo-1648141499246-97a0eb56c2fd?fm=jpg&q=60&w=3000&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxzZWFyY2h8Mnx8ZWdnJTIwZmFybXxlbnwwfHwwfHx8MA%3D%3D",
                Category = "Dairy",
                HarvestDate = System.DateTime.Now.AddDays(-1),
                FarmerProfile = new FarmerProfile { FarmName = "Happy Hens Farm", Certification = "Free-Range" }
            },
            new Product
            {
                Id = 4,
                Name = "Organic Carrots",
                Description = "Sweet organic carrots, perfect for cooking",
                Price = 60,
                StockQuantity = 0, // Out of stock
                ImageUrl = "https://images.unsplash.com/photo-1639086495429-d60e72c53c81?fm=jpg&q=60&w=3000&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxzZWFyY2h8MTV8fGNhcnJvdHN8ZW58MHx8MHx8fDA%3D",
                Category = "Vegetables",
                HarvestDate = System.DateTime.Now.AddDays(-3),
                FarmerProfile = new FarmerProfile { FarmName = "Green Valley Farm", Certification = "Organic" }
            }
        };

        // GET: /Products
        public IActionResult Index(string searchString, string categoryFilter)
        {
            // Start with all products
            var products = _products.AsQueryable();

            // Search by product name
            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.Name.ToLower().Contains(searchString.ToLower()));
            }

            // Filter by category
            if (!string.IsNullOrEmpty(categoryFilter))
            {
                products = products.Where(p => p.Category == categoryFilter);
            }

            // Pass the search/filter values back to the view to keep them in the form
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentCategory"] = categoryFilter;

            // Get distinct categories for the dropdown filter
            ViewBag.Categories = _products
                .Select(p => p.Category)
                .Distinct()
                .ToList();

            return View(products.ToList());
        }

        // GET: /Products/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = _products.FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
    }
}