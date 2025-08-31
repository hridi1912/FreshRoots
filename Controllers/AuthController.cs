using Microsoft.AspNetCore.Mvc;

namespace FreshRoots.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Auth/Login
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            // TODO: integrate with ASP.NET Identity
            if (email == "test@freshroots.com" && password == "123456")
            {
                TempData["Message"] = "Login successful!";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Invalid credentials!";
            return View();
        }

        // GET: /Auth/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Auth/Register
        [HttpPost]
        public IActionResult Register(string name, string email, string password, string role)
        {
            // TODO: save to database with Identity
            TempData["Message"] = $"Account created for {name} as {role}";
            return RedirectToAction("Login");
        }

        // GET: /Auth/Logout
        public IActionResult Logout()
        {
            // TODO: use SignInManager.SignOutAsync() with Identity
            TempData["Message"] = "Logged out successfully!";
            return RedirectToAction("Index", "Home");
        }
    }
}
