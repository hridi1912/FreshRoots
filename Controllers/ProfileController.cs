using FreshRoots.Data;
using FreshRoots.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class ProfileController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;

    public ProfileController(UserManager<ApplicationUser> userManager,
                             SignInManager<ApplicationUser> signInManager,
                             ApplicationDbContext db,
                             IWebHostEnvironment env)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _db = db;
        _env = env;
    }

    // ----------------- FULL PAGE -----------------
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var model = await BuildProfileModel(user);
        return View(model);
    }

    // ----------------- PARTIAL (GET) -----------------
    public async Task<IActionResult> ProfilePartial()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var model = await BuildProfileModel(user);
        return PartialView("_ProfilePartial", model);
    }

    // ----------------- PARTIAL (POST - JSON) -----------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProfilePartial(ProfileViewModel model, IFormFile? profileImageFile)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Json(new { success = false, message = "User not found." });

        // Email is read-only
        model.Email = user.Email;

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return Json(new { success = false, message = "Invalid input: " + string.Join(", ", errors) });
        }

        // --- Update AspNetUsers ---
        user.FullName = model.FullName;
        user.PhoneNumber = model.PhoneNumber;
        await _userManager.UpdateAsync(user);

        // --- Farmer update ---
        if (user.UserType == "Farmer")
        {
            var farmer = await _db.Farmers.FirstOrDefaultAsync(f => f.UserId == user.Id);

            if (farmer == null)
            {
                farmer = new Farmer
                {
                    UserId = user.Id,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    PickupLocation = model.PickupLocation,
                    ProfilePicture = null
                };
                await _db.Farmers.AddAsync(farmer);
            }
            else
            {
                farmer.FullName = model.FullName;
                farmer.PhoneNumber = model.PhoneNumber;
                farmer.Address = model.Address;
                farmer.PickupLocation = model.PickupLocation;
            }

            if (profileImageFile != null && profileImageFile.Length > 0)
            {
                farmer.ProfilePicture = await SaveImage(profileImageFile);
                model.ProfilePicture = farmer.ProfilePicture;
            }

            _db.Farmers.Update(farmer);
            ViewBag.ProfileType = "Farmer";
        }
        // --- Buyer update ---
        else if (user.UserType == "Buyer")
        {
            var buyer = await _db.Buyers.FirstOrDefaultAsync(b => b.UserId == user.Id);

            if (buyer == null)
            {
                buyer = new Buyer
                {
                    UserId = user.Id,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    ProfilePicture = null
                };
                await _db.Buyers.AddAsync(buyer);
            }
            else
            {
                buyer.FullName = model.FullName;
                buyer.PhoneNumber = model.PhoneNumber;
                buyer.Address = model.Address;
            }

            if (profileImageFile != null && profileImageFile.Length > 0)
            {
                buyer.ProfilePicture = await SaveImage(profileImageFile);
                model.ProfilePicture = buyer.ProfilePicture;
            }

            _db.Buyers.Update(buyer);
            ViewBag.ProfileType = "Buyer";
        }
        else
        {
            ViewBag.ProfileType = "User";
        }

        await _db.SaveChangesAsync();
        await _signInManager.RefreshSignInAsync(user);

        return Json(new
        {
            success = true,
            message = "Profile updated successfully!",
            profilePicture = model.ProfilePicture
        });
    }

    // ----------------- Helpers -----------------
    private async Task<ProfileViewModel> BuildProfileModel(ApplicationUser user)
    {
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

        return model;
    }

    private async Task<string> SaveImage(IFormFile imageFile)
    {
        var ext = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        if (!allowed.Contains(ext))
            throw new Exception("Only JPG, JPEG, PNG, WEBP or GIF files are allowed.");

        var uploadsRoot = Path.Combine(_env.WebRootPath!, "images", "profiles");
        Directory.CreateDirectory(uploadsRoot);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(uploadsRoot, fileName);

        using (var stream = new FileStream(fullPath, FileMode.Create))
            await imageFile.CopyToAsync(stream);

        return $"/images/profiles/{fileName}";
    }
}
