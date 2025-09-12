using FreshRoots.Models;
using FreshRoots.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FreshRoots.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        private const string AdminEmail = "adminFreshRoots@gmail.com";
        private const string AdminPassword = "12345678";
        private const string AdminRole = "Admin";

        public AuthController(UserManager<ApplicationUser> userManager,
                              SignInManager<ApplicationUser> signInManager,
                              RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
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
                FullName = model.FullName,
                UserType = model.Role
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Ensure the role exists
                if (!await _roleManager.RoleExistsAsync(model.Role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(model.Role));
                }

                // Assign role to the user
                await _userManager.AddToRoleAsync(user, model.Role);

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

            // 🔹 Special check for admin
            if (model.Email == AdminEmail && model.Password == AdminPassword)
            {
                var adminUser = await _userManager.FindByEmailAsync(AdminEmail);

                if (adminUser == null)
                {
                    // Create admin account automatically
                    adminUser = new ApplicationUser
                    {
                        UserName = AdminEmail,
                        Email = AdminEmail,
                        FullName = "System Administrator",
                        UserType = AdminRole
                    };

                    var createResult = await _userManager.CreateAsync(adminUser, AdminPassword);
                    if (!createResult.Succeeded)
                    {
                        ModelState.AddModelError("", "Failed to create admin account.");
                        return View(model);
                    }
                }

                // Ensure Admin role exists
                if (!await _roleManager.RoleExistsAsync(AdminRole))
                {
                    await _roleManager.CreateAsync(new IdentityRole(AdminRole));
                }

                // Assign role if not already
                if (!await _userManager.IsInRoleAsync(adminUser, AdminRole))
                {
                    await _userManager.AddToRoleAsync(adminUser, AdminRole);
                }

                // Sign in admin
                await _signInManager.SignInAsync(adminUser, model.RememberMe);

                return RedirectToAction("Index", "Admin"); // 🔹 Redirect to Admin Panel
            }

            // 🔹 Normal login for non-admin users
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false
            );

            if (result.Succeeded)
            {
                if (await _userManager.IsInRoleAsync(user, "Farmer"))
                    return RedirectToAction("Index", "Home");
                else
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
