using InvWebApp.Data;
using InvWebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvWebApp.Controllers
{
    [Authorize(Roles = "Admin,InventoryClerk")] // or just [Authorize]
    public class LocationsController : Controller
    {
        private readonly AppDbContext _db;
        public LocationsController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index()
            => View(await _db.Locations.AsNoTracking().OrderBy(l => l.Name).ToListAsync());

        public IActionResult Create() => View(new Location());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Location loc)
        {
            if (!ModelState.IsValid) return View(loc);
            _db.Locations.Add(loc);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var loc = await _db.Locations.FindAsync(id);
            return loc == null ? NotFound() : View(loc);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Location loc)
        {
            if (!ModelState.IsValid) return View(loc);
            _db.Update(loc);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var loc = await _db.Locations.FindAsync(id);
            if (loc != null) { _db.Locations.Remove(loc); await _db.SaveChangesAsync(); }
            return RedirectToAction(nameof(Index));
        }
    }
}
