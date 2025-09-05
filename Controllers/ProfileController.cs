using FreshRoots.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class ProfileController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public ProfileController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // GET: Profile Partial
    public async Task<IActionResult> ProfilePartial()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();
        return PartialView("_ProfilePartial", user);
    }

    // POST: Profile Partial (AJAX)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProfilePartial(ApplicationUser model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        // Update fields
        user.FullName = model.FullName;
        user.Email = model.Email;
        user.UserName = model.Email;
        user.PhoneNumber = model.PhoneNumber;

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            await _signInManager.RefreshSignInAsync(user);
            ViewBag.Message = "Profile updated successfully!";
        }
        else
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);
        }

        // Return the updated partial view
        return PartialView("_ProfilePartial", user);
    }
}