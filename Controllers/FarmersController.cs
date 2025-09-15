using FreshRoots.Data;
using FreshRoots.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Linq;

[Authorize(Roles = "Buyer")]
public class FarmersController : Controller
{
    private readonly ApplicationDbContext _db;

    public FarmersController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> FavouriteFarmersPartial()
    {
        // Example: get all farmers or only those favorited by the current user
        var favouriteFarmers = await _db.Users
            .Where(u => u.Role == "Farmer") // Adjust your condition
            .ToListAsync();

        return PartialView("_FavouriteFarmersPartial", favouriteFarmers);
    }
}
