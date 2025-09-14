using System.ComponentModel.DataAnnotations.Schema;

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

        // Order relationship
        public int OrderId { get; set; }
        public Order Order { get; set; }

        // Product relationship
        public int ProductId { get; set; }
        public Product Product { get; set; }

        // Farmer relationship
        public string FarmerId { get; set; }
        public ApplicationUser Farmer { get; set; }

        public int Quantity { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public string Status { get; set; } = "Pending";
    }
    
}
