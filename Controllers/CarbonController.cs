using FreshRoots.Data;
using Microsoft.AspNetCore.Mvc;

public class CarbonController : Controller
{
    private readonly ApplicationDbContext _db;

    public CarbonController(ApplicationDbContext db)
    {
        _db = db;
    }

    // Returns the HTML for carbon entries
    public IActionResult GetCarbonEntries()
    {
        var entries = _db.CarbonEntries
                         .OrderByDescending(c => c.EntryDate)
                         .ToList();
        return PartialView("_CarbonEntriesPartial", entries);
    }
}
