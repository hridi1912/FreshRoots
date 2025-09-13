using FreshRoots.Models;

namespace FreshRoots.ViewModels
{
    public class TopProductViewModel
    {
        public Product Product { get; set; }
        public int TotalSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
