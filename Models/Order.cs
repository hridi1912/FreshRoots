using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace FreshRoots.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string BuyerId { get; set; }
        public ApplicationUser Buyer { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Pending";

        public ICollection<OrderItem> OrderItems { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }
    }

    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        [ValidateNever]   
        public Order Order { get; set; }

        public int ProductId { get; set; }

        [ValidateNever]
        public Product Product { get; set; }

        public int FarmerId { get; set; }

        [ValidateNever]
        public Farmer Farmer { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public string Status { get; set; } = "Pending";
    }
}
