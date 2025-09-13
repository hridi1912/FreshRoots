using FreshRoots.Models;

namespace FreshRoots.ViewModels
{
    public class FarmerDashboardViewModel
    {
        public IEnumerable<Product> Products { get; set; }
        public DashboardStats Stats { get; set; }
    }

    public class DashboardStats
    {
        public int NewOrders { get; set; }
        public int ActiveProducts { get; set; }
        public int TotalCustomers { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
