using FreshRoots.Data;
using FreshRoots.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FreshRoots.Controllers
{
    [Authorize(Roles = "Buyer")]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // Add product to cart
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var cart = await _db.Carts.Include(c => c.Items)
                                      .ThenInclude(i => i.Product)
                                      .FirstOrDefaultAsync(c => c.BuyerId == user.Id);

            if (cart == null)
            {
                cart = new Cart { BuyerId = user.Id };
                _db.Carts.Add(cart);
                await _db.SaveChangesAsync();
            }

            var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (cartItem != null)
            {
                cartItem.Quantity += quantity; // increase quantity
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    ProductId = productId,
                    Quantity = quantity
                });
            }

            await _db.SaveChangesAsync();
            TempData["Msg"] = "Product added to cart!";
            return RedirectToAction("Index", "Products");
        }

        [HttpPost]
        [Authorize(Roles = "Buyer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int change)
        {
            var item = await _db.CartItems.Include(ci => ci.Product).FirstOrDefaultAsync(ci => ci.Id == cartItemId);
            if (item == null) return NotFound();

            item.Quantity += change;

            // Prevent quantity < 1
            if (item.Quantity < 1)
                item.Quantity = 1;

            // Prevent quantity > stock
            if (item.Quantity > item.Product.StockQuantity)
                item.Quantity = item.Product.StockQuantity;

            await _db.SaveChangesAsync();

            return RedirectToAction("Index"); // Cart page
        }

        // Show cart
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var cart = await _db.Carts.Include(c => c.Items)
                                      .ThenInclude(i => i.Product)
                                      .FirstOrDefaultAsync(c => c.BuyerId == user.Id);

            return View(cart);
        }

        // Remove item
        [HttpPost]
        public async Task<IActionResult> Remove(int cartItemId)
        {
            var item = await _db.CartItems.FirstOrDefaultAsync(i => i.Id == cartItemId);
            if (item != null)
            {
                _db.CartItems.Remove(item);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
