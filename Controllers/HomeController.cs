using InvWebApp.Data;
using InvWebApp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace InvWebApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Counts
            var materielCount = await _context.Materiels.CountAsync();
            var categoryCount = await _context.Categories.CountAsync();
            var serviceCount = await _context.Services.CountAsync();
            var userCount = await _context.Users.CountAsync();

            // Low stock (Quantity <= ReorderPoint). Uses Materiel.Quantity for simplicity.
            // Low stock (Quantity <= ReorderPoint)
            var lowStockQuery = _context.Materiels
                .Include(m => m.Categorie)
                .Include(m => m.Service)
                .Where(m => (m.ReorderPoint ?? 0) > 0 && m.Quantity <= (m.ReorderPoint ?? 0));

            var lowStockCount = await lowStockQuery.CountAsync();
            var lowStockTop = await lowStockQuery
                .OrderBy(m => (m.Quantity - (m.ReorderPoint ?? 0)))
                .Take(5)
                .ToListAsync();

            // Recent work orders + open count (Open or InProgress)
            var workOrdersTop = await _context.WorkOrders
                .Include(w => w.Materiel)
                .OrderByDescending(w => w.OpenDate)
                .Take(5)
                .ToListAsync();

            var openWorkOrdersCount = await _context.WorkOrders
                .CountAsync(w => w.Status == WorkOrderStatus.Open || w.Status == WorkOrderStatus.InProgress);

            // Existing lists (keep your design)
            var vm = new DashboardData
            {
                MaterielCount = materielCount,
                CategoryCount = categoryCount,
                ServiceCount = serviceCount,
                UserCount = userCount,
                LowStockCount = lowStockCount,
                OpenWorkOrdersCount = openWorkOrdersCount,

                CategorieList = await _context.Categories.OrderByDescending(x => x.Id).Take(5).ToListAsync(),
                ServiceList = await _context.Services.OrderByDescending(x => x.Id).Take(5).ToListAsync(),
                MaterielList = await _context.Materiels
                                    .Include(m => m.Categorie)
                                    .Include(m => m.Service)
                                    .OrderByDescending(x => x.Id)
                                    .Take(5).ToListAsync(),
                _LogList = await _context.LogLists.Include(u => u.User)
                                    .OrderByDescending(x => x.Id).Take(5).ToListAsync(),

                LowStockTop = lowStockTop,
                WorkOrdersTop = workOrdersTop
            };

            return View(vm);
        }

        public IActionResult Privacy() => View();

        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Admin");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
