using FreshRoots.Data;
using FreshRoots.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FreshRoots.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;

        }

        // ✅ Checkout page preview
        public async Task<IActionResult> Checkout()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            // Get cart items including product
            var cartItems = await _db.CartItems
                .Include(c => c.Product)
                .Include(c => c.Cart)
                .Where(c => c.Cart.BuyerId == user.Id)
                .ToListAsync();

            if (!cartItems.Any())
                return RedirectToAction("Index", "Cart");

            // Create order preview
            var orderPreview = new Order
            {
                BuyerId = user.Id,
                OrderDate = DateTime.Now,
                Status = "Pending",
                TotalAmount = cartItems.Sum(c => c.Quantity * c.Product.Price),
                OrderItems = cartItems.Select(c => new OrderItem
                {
                    ProductId = c.ProductId,
                    FarmerId = c.Product.FarmerId,
                    Quantity = c.Quantity,
                    Price = c.Product.Price,
                    Product = c.Product // ✅ include Product to avoid null
                }).ToList()
            };

            return View(orderPreview);
        }

        // ✅ Place Order (submit)
        [HttpPost]
        public async Task<IActionResult> PlaceOrder()
        {
            var user = await _userManager.GetUserAsync(User);
            var cartItems = await _db.CartItems
                .Include(c => c.Product)
                .Include(c => c.Cart)
                .Where(c => c.Cart.BuyerId == user.Id)
                .ToListAsync();

            if (!cartItems.Any())
                return RedirectToAction("Index", "Cart");

            var order = new Order
            {
                BuyerId = user.Id,
                OrderDate = DateTime.Now,
                Status = "Pending",
                TotalAmount = cartItems.Sum(c => c.Quantity * c.Product.Price),
                OrderItems = cartItems.Select(c => new OrderItem
                {
                    ProductId = c.ProductId,
                    FarmerId = c.Product.FarmerId,
                    Quantity = c.Quantity,
                    Price = c.Product.Price,
                    Product = c.Product
                }).ToList()
            };

            _db.Orders.Add(order);
            _db.CartItems.RemoveRange(cartItems);
            await _db.SaveChangesAsync();

            // Redirect to Order Confirmation page
            return RedirectToAction("OrderConfirmation", new { orderId = order.Id });
        }

        // ✅ Show buyer’s orders
        public async Task<IActionResult> MyOrders()
        {
            var user = await _userManager.GetUserAsync(User);

            var orders = await _db.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.BuyerId == user.Id)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        public IActionResult BuyerOrdersPartial()
        {
            var buyerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orders = _db.Orders
                .Where(o => o.BuyerId == buyerId)
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .ToList();

            return PartialView("BuyersOrderPartial", orders);
        }

    }
}
