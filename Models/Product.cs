﻿namespace FreshRoots.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string ImageUrl { get; set; }
        public string Category { get; set; }
        public DateTime HarvestDate { get; set; }
        public FarmerProfile FarmerProfile { get; set; }
    }

    public class FarmerProfile
    {
        public string FarmName { get; set; }
        public string Certification { get; set; }
    }
}
