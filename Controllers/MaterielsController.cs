using InvWebApp.Data;
using InvWebApp.Extentions; // for User.getUserId(_context)
using InvWebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace InvWebApp.Controllers
{
    [Authorize]
    public class MaterielsController : Controller
    {
        private readonly AppDbContext _context;

        public MaterielsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Materiels
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Materiels
                .Include(s => s.Service)
                .Include(g => g.serviceGroup)
                .Include(c => c.Categorie);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Materiels/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Materiels == null)
                return NotFound();

            var materiel = await _context.Materiels
                .Include(m => m.Categorie)
                .Include(m => m.Service)
                .Include(m => m.serviceGroup)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (materiel == null)
                return NotFound();

            return View(materiel);
        }

        // -------- CREATE --------

        // GET: Materiels/Create
        public async Task<IActionResult> Create()
        {
            await LoadDropdownsAsync();
            // Default Kind = Consumable so the Equipment block stays hidden initially
            return View(new Materiel { Kind = MaterielKind.Consumable });
        }

        // POST: Materiels/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Materiel materiel)
        {
            // Unique serial number check (only when provided)
            if (!string.IsNullOrWhiteSpace(materiel.SerialNumber))
            {
                var exists = await _context.Materiels
                    .AnyAsync(u => u.SerialNumber != null &&
                                   u.SerialNumber.ToLower() == materiel.SerialNumber.ToLower());
                if (exists)
                    ModelState.AddModelError("SerialNumber", "Serial number already exists.");
            }

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync();
                return View(materiel);
            }

            // --- IMPORTANT: set creator (Option 1) ---
            // Your extension returns an int (typically). Guard it just in case.
            int? currentUserId = null;
            try
            {
                // If your extension returns int (not nullable), change to: currentUserId = User.getUserId(_context);
                currentUserId = User.getUserId(_context);
            }
            catch { /* ignore, we'll fallback below */ }

            if (currentUserId.HasValue && currentUserId.Value > 0)
            {
                materiel.UserId = currentUserId.Value;
            }
            else
            {
                // Fallback to any existing user (e.g., Admin) so DB NOT NULL constraint is satisfied
                var anyUserId = await _context.Users.Select(u => u.Id).FirstOrDefaultAsync();
                if (anyUserId > 0)
                    materiel.UserId = anyUserId;
            }
            // -----------------------------------------

            materiel.CreatedDate = DateTime.Now;

            _context.Materiels.Add(materiel);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // API create (kept as-is)
        [AllowAnonymous]
        [HttpPost]
        [Route("/api/create")]
        public async Task<IActionResult> CreateMateriel(
            [Bind("Id,MaterielName,CreatedDate,SerialNumber,InventoryNumber,Quantity,CategorieId,ServiceId")]
            Materiel materiel)
        {
            _context.Add(materiel);
            await _context.SaveChangesAsync();
            return Ok(materiel);
        }

        // -------- EDIT --------

        // GET: Materiels/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Materiels == null)
                return NotFound();

            var materiel = await _context.Materiels.FindAsync(id);
            if (materiel == null)
                return NotFound();

            await LoadDropdownsAsync();
            return View(materiel);
        }

        // POST: Materiels/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Materiel materiel)
        {
            if (id != materiel.Id)
                return NotFound();

            // Unique serial number check (ignore this record)
            if (!string.IsNullOrWhiteSpace(materiel.SerialNumber))
            {
                var exists = await _context.Materiels
                    .AnyAsync(u => u.SerialNumber != null &&
                                   u.SerialNumber.ToLower() == materiel.SerialNumber.ToLower() &&
                                   u.Id != materiel.Id);
                if (exists)
                    ModelState.AddModelError("SerialNumber", "Serial number already exists.");
            }

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync();
                return View(materiel);
            }

            try
            {
                // If you want to record who last modified, keep this:
                materiel.UserId = User.getUserId(_context);

                _context.Update(materiel);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MaterielExists(materiel.Id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // -------- DELETE --------

        // GET: Materiels/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Materiels == null)
                return NotFound();

            var materiel = await _context.Materiels
                .Include(m => m.Categorie)
                .Include(m => m.Service)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (materiel == null)
                return NotFound();

            return View(materiel);
        }

        // POST: Materiels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Materiels == null)
                return Problem("Entity set 'AppDbContext.Materiels' is null.");

            var materiel = await _context.Materiels.FindAsync(id);
            if (materiel != null)
                _context.Materiels.Remove(materiel);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MaterielExists(int id)
            => (_context.Materiels?.Any(e => e.Id == id)).GetValueOrDefault();

        // AJAX: load groups for a service
        [HttpGet]
        public IActionResult GetGroups(int serviceId)
        {
            var groups = _context.serviceGroups
                .Where(s => s.ServiceId == serviceId)
                .Select(g => new { Id = g.Id, GroupName = g.GroupName })
                .ToList();
            return Json(groups);
        }

        // ---- Dropdown helper (null-safe) ----
        private async Task LoadDropdownsAsync()
        {
            // Categories
            var categories = await _context.Categories
                .AsNoTracking()
                .Select(c => new { c.Id, Name = c.CategorieName ?? "" })
                .OrderBy(c => c.Name)
                .ToListAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");

            // Services
            var services = await _context.Services
                .AsNoTracking()
                .Select(s => new { s.Id, Name = s.ServiceName ?? "" })
                .OrderBy(s => s.Name)
                .ToListAsync();
            ViewBag.Services = new SelectList(services, "Id", "Name");

            // Groups
            var groups = await _context.serviceGroups
                .AsNoTracking()
                .Select(g => new { g.Id, Name = g.GroupName ?? "" })
                .OrderBy(g => g.Name)
                .ToListAsync();
            ViewBag.Groups = new SelectList(groups, "Id", "Name");
        }
    }
}
