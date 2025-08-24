using InvWebApp.Data;
using InvWebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InvWebApp.Controllers
{
    [Authorize(Roles = "Admin,Technician")]
    public class WorkOrdersController : Controller
    {
        private readonly AppDbContext _db;
        public WorkOrdersController(AppDbContext db) => _db = db;

        private async Task LoadMaterielsAsync()
        {
            // only equipment materiels for the dropdown
            var items = await _db.Materiels
                .Where(m => m.Kind == MaterielKind.Equipment)
                .OrderBy(m => m.MaterielName)
                .Select(m => new { m.Id, m.MaterielName })
                .ToListAsync();
            ViewBag.Materiels = new SelectList(items, "Id", "MaterielName");
        }

        // GET: WorkOrders
        public async Task<IActionResult> Index()
        {
            var list = await _db.WorkOrders
                .Include(w => w.Materiel)
                .OrderByDescending(w => w.OpenDate)
                .ToListAsync();
            return View(list);
        }

        // GET: WorkOrders/Create
        public async Task<IActionResult> Create()
        {
            await LoadMaterielsAsync();
            return View(new WorkOrder());
        }

        // POST: WorkOrders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WorkOrder w)
        {
            // Fallback: if binder left MaterielId at 0, try to read the posted value directly.
            if (w.MaterielId <= 0 && int.TryParse(Request.Form["MaterielId"], out var mid))
                w.MaterielId = mid;

            // Ensure a valid selection
            if (w.MaterielId <= 0)
                ModelState.AddModelError(nameof(WorkOrder.MaterielId), "Please select a materiel.");

            if (!ModelState.IsValid)
            {
                await LoadMaterielsAsync();
                return View(w);
            }

            _db.WorkOrders.Add(w);
            await _db.SaveChangesAsync();

            // Optional: flip materiel status when a WO is opened
            var m = await _db.Materiels.FindAsync(w.MaterielId);
            if (m != null && (m.Status == null || m.Status != EquipmentStatus.InMaintenance))
            {
                m.Status = EquipmentStatus.InMaintenance;
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }


        // GET: WorkOrders/Edit/5  (used here as "Update / Start / Assign")
        public async Task<IActionResult> Edit(int id)
        {
            var w = await _db.WorkOrders.Include(x => x.Materiel).FirstOrDefaultAsync(x => x.Id == id);
            if (w == null) return NotFound();
            await LoadMaterielsAsync();
            return View(w);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, WorkOrder w)
        {
            if (id != w.Id) return NotFound();
            if (!ModelState.IsValid)
            {
                await LoadMaterielsAsync();
                return View(w);
            }

            _db.Update(w);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: WorkOrders/Complete/5
        public async Task<IActionResult> Complete(int id)
        {
            var w = await _db.WorkOrders.Include(x => x.Materiel).FirstOrDefaultAsync(x => x.Id == id);
            if (w == null) return NotFound();
            return View(w);
        }

        // POST: WorkOrders/Complete/5
        [HttpPost, ActionName("Complete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteConfirmed(int id, string? resolutionNote, DateTime? nextServiceDate)
        {
            var w = await _db.WorkOrders.FindAsync(id);
            if (w == null) return NotFound();

            w.Status = WorkOrderStatus.Completed;
            w.CompletedDate = DateTime.UtcNow;
            w.ResolutionNote = resolutionNote;
            await _db.SaveChangesAsync();

            // Optional: set materiel status back to Active + schedule next service (demo-friendly)
            var m = await _db.Materiels.FindAsync(w.MaterielId);
            if (m != null)
            {
                m.Status = EquipmentStatus.Active;
                if (nextServiceDate.HasValue)
                {
                    m.LastServiceDate = DateTime.UtcNow;
                    m.NextServiceDate = nextServiceDate;
                }
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var w = await _db.WorkOrders.Include(x => x.Materiel).FirstOrDefaultAsync(x => x.Id == id);
            if (w == null) return NotFound();
            return View(w);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var w = await _db.WorkOrders.FindAsync(id);
            if (w != null) _db.WorkOrders.Remove(w);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
