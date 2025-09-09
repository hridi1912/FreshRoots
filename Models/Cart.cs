// Models/Cart.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreshRoots.Models
{
    public class Cart
    {
        public int Id { get; set; }

        [Required]
        public string BuyerId { get; set; }   // FK to AspNetUsers

        public ApplicationUser Buyer { get; set; }

        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }

    public class CartItem
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        public Product Product { get; set; }

        [Range(1, 1000)]
        public int Quantity { get; set; }

        // FK to Cart
        [Required]
        public int CartId { get; set; }
        public Cart Cart { get; set; }
    }
}
