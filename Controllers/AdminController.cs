using FreshRoots.Data;
using FreshRoots.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FreshRoots.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AdminController(ApplicationDbContext db)
        {
            _db = db;
        }

        // ================= DASHBOARD =================
        public IActionResult Index()
        {
            return View();
        }

        // ================= USERS =================
        public async Task<IActionResult> Users()
        {
            var users = await _db.Users.ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(ApplicationUser model)
        {
            if (ModelState.IsValid)
            {
                _db.Users.Update(model);
                await _db.SaveChangesAsync();
                return RedirectToAction("Users");
            }
            return View(model);
        }

        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost, ActionName("DeleteUser")]
        public async Task<IActionResult> DeleteUserConfirmed(string id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            if (user.UserType == "Farmer")
            {
                var farmer = await _db.Farmers.FirstOrDefaultAsync(f => f.UserId == id);
                if (farmer != null)
                {
                    // Delete farmer’s order items first
                    var farmerProductIds = await _db.Products
                        .Where(p => p.FarmerId == farmer.FarmerId)
                        .Select(p => p.Id)
                        .ToListAsync();

                    var orderItems = _db.OrderItems.Where(oi => farmerProductIds.Contains(oi.ProductId));
                    _db.OrderItems.RemoveRange(orderItems);

                    // Delete farmer’s products
                    var products = _db.Products.Where(p => p.FarmerId == farmer.FarmerId);
                    _db.Products.RemoveRange(products);

                    // Delete farmer row
                    _db.Farmers.Remove(farmer);
                }
            }
            else if (user.UserType == "Buyer")
            {
                var buyer = await _db.Buyers.FirstOrDefaultAsync(b => b.UserId == id);
                if (buyer != null)
                {
                    _db.Buyers.Remove(buyer);
                }
            }

            // Delete the user itself
            _db.Users.Remove(user);

            await _db.SaveChangesAsync();
            return RedirectToAction("Users");
        }

        // ================= FARMERS =================
        public async Task<IActionResult> Farmers()
        {
            var farmers = await _db.Farmers.Include(f => f.User).ToListAsync();
            return View(farmers);
        }

        public async Task<IActionResult> EditFarmer(int id)
        {
            var farmer = await _db.Farmers.FindAsync(id);
            if (farmer == null) return NotFound();
            return View(farmer);
        }

        [HttpPost]
        public async Task<IActionResult> EditFarmer(Farmer model)
        {
            if (ModelState.IsValid)
            {
                _db.Farmers.Update(model);
                await _db.SaveChangesAsync();
                return RedirectToAction("Farmers");
            }
            return View(model);
        }

        public async Task<IActionResult> DeleteFarmer(int id)
        {
            var farmer = await _db.Farmers.FindAsync(id);
            if (farmer == null) return NotFound();

            return View(farmer);
        }

        [HttpPost, ActionName("DeleteFarmer")]
        public async Task<IActionResult> DeleteFarmerConfirmed(int id)
        {
            var farmer = await _db.Farmers.FindAsync(id);
            if (farmer == null) return NotFound();

            // Delete order items linked to farmer’s products
            var farmerProductIds = await _db.Products
                .Where(p => p.FarmerId == id)
                .Select(p => p.Id)
                .ToListAsync();

            var orderItems = _db.OrderItems.Where(oi => farmerProductIds.Contains(oi.ProductId));
            _db.OrderItems.RemoveRange(orderItems);

            // Delete farmer’s products
            var products = _db.Products.Where(p => p.FarmerId == id);
            _db.Products.RemoveRange(products);

            // Delete farmer row
            _db.Farmers.Remove(farmer);

            // Delete linked user
            var user = await _db.Users.FindAsync(farmer.UserId);
            if (user != null)
            {
                _db.Users.Remove(user);
            }

            await _db.SaveChangesAsync();
            return RedirectToAction("Farmers");
        }

        // ================= BUYERS =================
        public async Task<IActionResult> Buyers()
        {
            var buyers = await _db.Buyers.Include(b => b.User).ToListAsync();
            return View(buyers);
        }

        public async Task<IActionResult> EditBuyer(int id)
        {
            var buyer = await _db.Buyers.FindAsync(id);
            if (buyer == null) return NotFound();
            return View(buyer);
        }

        [HttpPost]
        public async Task<IActionResult> EditBuyer(Buyer model)
        {
            if (ModelState.IsValid)
            {
                _db.Buyers.Update(model);
                await _db.SaveChangesAsync();
                return RedirectToAction("Buyers");
            }
            return View(model);
        }

        public async Task<IActionResult> DeleteBuyer(int id)
        {
            var buyer = await _db.Buyers.FindAsync(id);
            if (buyer == null) return NotFound();
            return View(buyer);
        }

        [HttpPost, ActionName("DeleteBuyer")]
        public async Task<IActionResult> DeleteBuyerConfirmed(int id)
        {
            var buyer = await _db.Buyers.FindAsync(id);
            if (buyer == null) return NotFound();

            _db.Buyers.Remove(buyer);

            var user = await _db.Users.FindAsync(buyer.UserId);
            if (user != null)
            {
                _db.Users.Remove(user);
            }

            await _db.SaveChangesAsync();
            return RedirectToAction("Buyers");
        }

        // ================= PRODUCTS =================
        public async Task<IActionResult> Products()
        {
            var products = await _db.Products.ToListAsync();
            return View(products);
        }

        public async Task<IActionResult> EditProduct(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> EditProduct(Product model)
        {
            if (ModelState.IsValid)
            {
                _db.Products.Update(model);
                await _db.SaveChangesAsync();
                return RedirectToAction("Products");
            }
            return View(model);
        }

        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost, ActionName("DeleteProduct")]
        public async Task<IActionResult> DeleteProductConfirmed(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();

            // ✅ Delete order items first
            var orderItems = _db.OrderItems.Where(oi => oi.ProductId == id);
            _db.OrderItems.RemoveRange(orderItems);

            _db.Products.Remove(product);
            await _db.SaveChangesAsync();
            return RedirectToAction("Products");
        }

        // ================= ORDERS =================
        public async Task<IActionResult> Orders()
        {
            var orderItems = await _db.OrderItems
                .Include(oi => oi.Product)
                .Include(oi => oi.Order)
                .ToListAsync();

            return View(orderItems);
        }

        public async Task<IActionResult> EditOrderItem(int id)
        {
            var orderItem = await _db.OrderItems
                .Include(oi => oi.Order)
                .FirstOrDefaultAsync(oi => oi.Id == id);

            if (orderItem == null) return NotFound();

            return View(orderItem);
        }

        [HttpPost]
        public async Task<IActionResult> EditOrderItem(OrderItem model)
        {
            if (ModelState.IsValid)
            {
                var orderItem = await _db.OrderItems.FindAsync(model.Id);
                if (orderItem == null) return NotFound();

                orderItem.Price = model.Price;
                orderItem.Quantity = model.Quantity;
                orderItem.Status = model.Status;

                _db.OrderItems.Update(orderItem);
                await _db.SaveChangesAsync();

                return RedirectToAction("Orders");
            }
            return View(model);
        }

        public async Task<IActionResult> DeleteOrderItem(int id)
        {
            var orderItem = await _db.OrderItems
                .Include(oi => oi.Order)
                .FirstOrDefaultAsync(oi => oi.Id == id);

            if (orderItem == null) return NotFound();

            return View(orderItem);
        }

        [HttpPost, ActionName("DeleteOrderItem")]
        public async Task<IActionResult> DeleteOrderItemConfirmed(int id)
        {
            var orderItem = await _db.OrderItems
                .Include(oi => oi.Order)
                .FirstOrDefaultAsync(oi => oi.Id == id);

            if (orderItem == null) return NotFound();

            var order = orderItem.Order;

            _db.OrderItems.Remove(orderItem);

            var remainingItems = await _db.OrderItems
                .Where(oi => oi.OrderId == order.Id)
                .ToListAsync();

            if (!remainingItems.Any())
            {
                _db.Orders.Remove(order);
            }

            await _db.SaveChangesAsync();
            return RedirectToAction("Orders");
        }
    }
}
