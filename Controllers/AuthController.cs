using FreshRoots.Data;
using FreshRoots.Models;
using FreshRoots.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace FreshRoots.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;

        private const string AdminEmail = "adminFreshRoots@gmail.com";
        private const string AdminPassword = "12345678";
        private const string AdminRole = "Admin";

        public AuthController(UserManager<ApplicationUser> userManager,
                              SignInManager<ApplicationUser> signInManager,
                              RoleManager<IdentityRole> roleManager,
                              ApplicationDbContext db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _db = db;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                FullName = model.FullName,
                UserType = model.Role,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync(model.Role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(model.Role));
                }

                await _userManager.AddToRoleAsync(user, model.Role);

                // Insert into Farmer or Buyer table
                if (model.Role == "Farmer")
                {
                    var farmer = new Farmer
                    {
                        UserId = user.Id,
                        FullName = model.FullName,
                        PhoneNumber = model.PhoneNumber,
                        Address = null,
                        PickupLocation = null,
                        ProfilePicture = null
                    };
                    _db.Farmers.Add(farmer);
                }
                else if (model.Role == "Buyer")
                {
                    var buyer = new Buyer
                    {
                        UserId = user.Id,
                        FullName = model.FullName,
                        PhoneNumber = model.PhoneNumber,
                        Address = null,
                        ProfilePicture = null
                    };
                    _db.Buyers.Add(buyer);
                }

                await _db.SaveChangesAsync();

                TempData["Message"] = "Registration successful! Please login.";
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Special check for admin
            if (model.Email == AdminEmail && model.Password == AdminPassword)
            {
                var adminUser = await _userManager.FindByEmailAsync(AdminEmail);

                if (adminUser == null)
                {
                    adminUser = new ApplicationUser
                    {
                        UserName = AdminEmail,
                        Email = AdminEmail,
                        PhoneNumber = null,
                        FullName = "System Administrator",
                        UserType = AdminRole,
                        CreatedAt = DateTime.UtcNow
                    };

                    var createResult = await _userManager.CreateAsync(adminUser, AdminPassword);
                    if (!createResult.Succeeded)
                    {
                        ModelState.AddModelError("", "Failed to create admin account.");
                        return View(model);
                    }
                }

                if (!await _roleManager.RoleExistsAsync(AdminRole))
                {
                    await _roleManager.CreateAsync(new IdentityRole(AdminRole));
                }

                if (!await _userManager.IsInRoleAsync(adminUser, AdminRole))
                {
                    await _userManager.AddToRoleAsync(adminUser, AdminRole);
                }

                await _signInManager.SignInAsync(adminUser, model.RememberMe);
                return RedirectToAction("Index", "Admin");
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return View(model);
            }

            // 🔹 Check if chosen role matches stored role
            if (!string.Equals(user.UserType, model.Role, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", $"You have registered as {user.UserType}. Please select {user.UserType}.");
                return View(model);
            }

            var loginResult = await _signInManager.PasswordSignInAsync(
                user.UserName,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false
            );

            if (loginResult.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid login attempt.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
