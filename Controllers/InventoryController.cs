using System;
using System.Threading.Tasks;
using InvWebApp.Data;
using InvWebApp.Services.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvWebApp.Controllers
{
    [Authorize]
    public class InventoryController : Controller
    {
        private readonly IInventoryService _svc;
        private readonly AppDbContext _db;
        public InventoryController(IInventoryService svc, AppDbContext db) { _svc = svc; _db = db; }

        public async Task<IActionResult> Index(int? materielId, int? locationId)
        {
            ViewBag.Materiels = await _db.Materiels.AsNoTracking().ToListAsync();
            ViewBag.Locations = await _db.Locations.AsNoTracking().ToListAsync();
            var model = await _svc.GetStocksAsync(materielId, locationId);
            return View(model);
        }

        public async Task<IActionResult> Receive() { await Load(); return View(); }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Receive(int materielId, int locationId, int quantity, string? batchNo, DateTime? expiry)
        {
            await _svc.ReceiveAsync(materielId, locationId, quantity, batchNo, expiry, User.Identity?.Name ?? "system");
            TempData["ok"] = "Received.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Issue() { await Load(); return View(); }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Issue(int materielId, int locationId, int quantity, string? reason)
        {
            await _svc.IssueAsync(materielId, locationId, quantity, User.Identity?.Name ?? "system", reason);
            TempData["ok"] = "Issued.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Transfer() { await Load(); return View(); }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Transfer(int materielId, int fromLocationId, int toLocationId, int quantity)
        {
            await _svc.TransferAsync(materielId, fromLocationId, toLocationId, quantity, User.Identity?.Name ?? "system");
            TempData["ok"] = "Transferred.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Adjust() { await Load(); return View(); }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Adjust(int materielId, int locationId, int delta, string reason)
        {
            await _svc.AdjustAsync(materielId, locationId, delta, User.Identity?.Name ?? "system", reason);
            TempData["ok"] = "Adjusted.";
            return RedirectToAction(nameof(Index));
        }

        private async Task Load()
        {
            ViewBag.Materiels = await _db.Materiels.AsNoTracking().ToListAsync();
            ViewBag.Locations = await _db.Locations.AsNoTracking().ToListAsync();
        }
    }
}
