using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreshRoots.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required, StringLength(120)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [Range(0.01, 999999)]
        public decimal Price { get; set; }

        [Range(0, 100000)]
        public int StockQuantity { get; set; }

        public string? ImageUrl { get; set; }

        [Required, StringLength(64)]
        public string Category { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime HarvestDate { get; set; } = DateTime.Today;

        
        [Required]
        [ForeignKey("Farmer")]
        public int FarmerId { get; set; }
        public Farmer? Farmer { get; set; }
        

        public FarmerProfile FarmerProfile { get; set; } = new FarmerProfile();
    }

    public class FarmerProfile
    {
        [StringLength(120)]
        public string FarmName { get; set; }

        [StringLength(120)]
        public string Certification { get; set; }
    }
}
