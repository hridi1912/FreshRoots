using System;
using System.Collections.Generic;
using FreshRoots.Models;

namespace FreshRoots.Services
{
    // Central in-memory store so multiple controllers can share one list
    public static class InMemoryProductStore
    {
        // Seed with a couple of samples (edit as you like)
        public static readonly List<Product> Products = new()
        {
            new Product
            {
                Id = 1,
                Name = "Organic Tomatoes",
                Description = "Fresh organic tomatoes, harvested yesterday",
                Price = 40,
                StockQuantity = 20,
                ImageUrl = "/images/products/sample-tomatoes.jpg",
                Category = "Vegetables",
                HarvestDate = DateTime.Today.AddDays(-1),
                FarmerProfile = new FarmerProfile { FarmName = "Green Valley Farm", Certification = "Organic" }
            },
            new Product
            {
                Id = 2,
                Name = "Fresh Apples",
                Description = "Sweet and crunchy apples from our orchard",
                Price = 240,
                StockQuantity = 15,
                ImageUrl = "/images/products/sample-apples.jpg",
                Category = "Fruits",
                HarvestDate = DateTime.Today.AddDays(-2),
                FarmerProfile = new FarmerProfile { FarmName = "Sunny Orchard", Certification = "Local" }
            }
        };
    }
}
