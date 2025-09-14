using System;
using System.Linq;
using FreshRoots.Data;
using FreshRoots.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FreshRoots.Controllers
{
    public class CustomersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CustomersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Shows buyers who actually purchased this farmer's items
        public IActionResult FarmerCustomers()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("No logged-in user.");

                // Find the farmerId (int) for this user
                var farmerId = _context.Farmers
                    .Where(f => f.UserId == userId)
                    .Select(f => f.FarmerId)
                    .SingleOrDefault();

                if (farmerId == 0)
                    return NotFound("Farmer not found for this user.");

                // 1) Which orders contain items for this farmer?
                var orderIds = _context.OrderItems
                    .Where(oi => oi.FarmerId == farmerId)     // ✅ uses OrderItems.FarmerId (int)
                    .Select(oi => oi.OrderId)
                    .Distinct()
                    .ToList();

                // 2) From those orders, collect unique Buyer *UserIds* (string)
                var buyerUserIds = _context.Orders
                    .Where(o => orderIds.Contains(o.Id))
                    .Select(o => o.BuyerId)                   // ✅ string (AspNetUsers.Id)
                    .Distinct()
                    .ToList();

                // 3) Pull Buyers via Buyers.UserId (string)
                var customers = _context.Buyers
                    .Where(b => buyerUserIds.Contains(b.UserId))
                    .OrderBy(b => b.FullName)
                    .ToList();

                return PartialView("_FarmerCustomersPartial", customers);
            }
            catch (Exception ex)
            {
                // TEMP: surface the exact cause so you can see *why* the 500 happens
                // Remove this and show a friendly page once it works.
                var msg = $"Customers/FarmerCustomers failed: {ex.Message}";
                if (ex.InnerException != null) msg += $" | Inner: {ex.InnerException.Message}";
                return StatusCode(500, msg);
            }
        }
    }
}
