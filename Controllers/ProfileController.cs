using FreshRoots.Data;
using FreshRoots.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class ProfileController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _db;

    public ProfileController(UserManager<ApplicationUser> userManager,
                             SignInManager<ApplicationUser> signInManager,
                             ApplicationDbContext db)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _db = db;
    }

    // Full page
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var model = new ProfileViewModel
        {
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            Email = user.Email
        };

        if (user.UserType == "Farmer")
        {
            var farmer = await _db.Farmers.FirstOrDefaultAsync(f => f.UserId == user.Id);
            if (farmer != null)
            {
                model.Address = farmer.Address;
                model.ProfilePicture = farmer.ProfilePicture;
                model.PickupLocation = farmer.PickupLocation;
            }
            ViewBag.ProfileType = "Farmer";
        }
        else if (user.UserType == "Buyer")
        {
            var buyer = await _db.Buyers.FirstOrDefaultAsync(b => b.UserId == user.Id);
            if (buyer != null)
            {
                model.Address = buyer.Address;
                model.ProfilePicture = buyer.ProfilePicture;
            }
            ViewBag.ProfileType = "Buyer";
        }
        else
        {
            ViewBag.ProfileType = "User";
        }

        return View(model);
    }

    // Partial (used by dashboard tab)
    public async Task<IActionResult> ProfilePartial()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var model = new ProfileViewModel
        {
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            Email = user.Email
        };

        if (user.UserType == "Farmer")
        {
            var farmer = await _db.Farmers.FirstOrDefaultAsync(f => f.UserId == user.Id);
            if (farmer != null)
            {
                model.Address = farmer.Address;
                model.ProfilePicture = farmer.ProfilePicture;
                model.PickupLocation = farmer.PickupLocation;
            }
            ViewBag.ProfileType = "Farmer";
        }
        else if (user.UserType == "Buyer")
        {
            var buyer = await _db.Buyers.FirstOrDefaultAsync(b => b.UserId == user.Id);
            if (buyer != null)
            {
                model.Address = buyer.Address;
                model.ProfilePicture = buyer.ProfilePicture;
            }
            ViewBag.ProfileType = "Buyer";
        }
        else
        {
            ViewBag.ProfileType = "User";
        }

        return PartialView("_ProfilePartial", model);
    }

    // POST: Update (returns the UPDATED partial so dashboard can swap it in)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProfilePartial(ProfileViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        // Email is read-only; always set from user to satisfy any scripts that expect it
        model.Email = user.Email;

        if (!ModelState.IsValid)
        {
            ViewBag.ProfileType = user.UserType ?? "User";
            return PartialView("_ProfilePartial", model);
        }

        // Update AspNetUsers
        user.FullName = model.FullName;
        user.PhoneNumber = model.PhoneNumber;
        await _userManager.UpdateAsync(user);

        if (user.UserType == "Farmer")
        {
            var farmer = await _db.Farmers.FirstOrDefaultAsync(f => f.UserId == user.Id);
            if (farmer != null)
            {
                farmer.FullName = model.FullName;
                farmer.PhoneNumber = model.PhoneNumber;
                farmer.Address = model.Address;
                farmer.ProfilePicture = model.ProfilePicture;
                farmer.PickupLocation = model.PickupLocation;
                _db.Farmers.Update(farmer);
            }
            ViewBag.ProfileType = "Farmer";
        }
        else if (user.UserType == "Buyer")
        {
            var buyer = await _db.Buyers.FirstOrDefaultAsync(b => b.UserId == user.Id);
            if (buyer != null)
            {
                buyer.FullName = model.FullName;
                buyer.PhoneNumber = model.PhoneNumber;
                buyer.Address = model.Address;
                buyer.ProfilePicture = model.ProfilePicture;
                _db.Buyers.Update(buyer);
            }
            ViewBag.ProfileType = "Buyer";
        }
        else
        {
            ViewBag.ProfileType = "User";
        }

        await _db.SaveChangesAsync();
        await _signInManager.RefreshSignInAsync(user);

        ViewBag.Message = "Profile updated successfully!";
        return PartialView("_ProfilePartial", model); // stay inside dashboard tab
    }
}
