using FreshRoots.Data;
using FreshRoots.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FreshRoots.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;

        public OrdersController(UserManager<ApplicationUser> userManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        // Favourite Farmers partial view
        public async Task<IActionResult> FavouriteFarmersPartial()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return PartialView("_FavouriteFarmersPartial", new List<ApplicationUser>());

            // Get unique farmer IDs from buyer's past orders
            var farmerIds = await _db.Orders
                .Where(o => o.BuyerId == user.Id)
                .SelectMany(o => o.OrderItems)
                .Select(oi => oi.FarmerId)
                .Distinct()
                .ToListAsync();

            // Get farmer details
            var farmers = await _db.Users
                .Where(u => farmerIds.Contains(u.Id))
                .ToListAsync();

            return PartialView("_FavouriteFarmersPartial", farmers);
        }
    }
}
