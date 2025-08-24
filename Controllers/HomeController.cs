using InvWebApp.Data;
using InvWebApp.Extentions;
using InvWebApp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
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

        [Authorize]
        public async Task<IActionResult> Index()
        {
            // Check if user is authenticated
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Admin");
            }

            try
            {
                var vm = new DashboardData();
                var userId = User.getUserId(_context);
                var userRoles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

                // Common data for all roles
                vm.MaterielCount = await _context.Materiels.CountAsync();
                vm.ServiceCount = await _context.Services.CountAsync();

                // Role-specific data
                if (userRoles.Contains("Admin") || userRoles.Contains("InventoryManager") || userRoles.Contains("Technician"))
                {
                    vm.CategoryCount = await _context.Categories.CountAsync();

                    // Fix low stock calculation
                    vm.LowStockCount = await _context.Materiels
                        .Where(m => m.Quantity <= (m.ReorderPoint ?? 0))
                        .CountAsync();

                    vm.LowStockTop = await _context.Materiels
                        .Where(m => m.Quantity <= (m.ReorderPoint ?? 0))
                        .OrderBy(m => m.Quantity)
                        .Take(5)
                        .Select(m => new DashboardData.LowStockItem
                        {
                            MaterielName = m.MaterielName,
                            Quantity = m.Quantity,
                            ReorderPoint = m.ReorderPoint,
                            Location = "Storage" // Default value
                        })
                        .ToListAsync();
                }

                if (userRoles.Contains("Admin") || userRoles.Contains("Technician"))
                {
                    vm.OpenWorkOrdersCount = await _context.WorkOrders
                        .Where(w => w.Status != WorkOrderStatus.Completed)
                        .CountAsync();

                    vm.WorkOrdersTop = await _context.WorkOrders
                        .Include(w => w.Materiel)
                        .Where(w => w.Status != WorkOrderStatus.Completed)
                        .OrderByDescending(w => w.Priority)
                        .ThenBy(w => w.OpenDate)
                        .Take(5)
                        .ToListAsync();
                }

                if (userRoles.Contains("Technician"))
                {
                    // Simplified - show all open work orders for technicians
                    vm.MyAssignedWorkOrders = await _context.WorkOrders
                        .Include(w => w.Materiel)
                        .Where(w => w.Status != WorkOrderStatus.Completed)
                        .OrderByDescending(w => w.Priority)
                        .Take(5)
                        .ToListAsync();
                }

                if (userRoles.Contains("Admin"))
                {
                    vm.UserCount = await _context.Users.CountAsync();
                    vm._LogList = await _context.LogLists
                        .Include(l => l.User)
                        .OrderByDescending(l => l.LogDate)
                        .Take(10)
                        .ToListAsync();
                }

                // FIXED: Correct enum comparison for nullable EquipmentStatus
                vm.OperationalEquipmentCount = await _context.Materiels
                    .Where(m => m.Status.HasValue && m.Status.Value == EquipmentStatus.Active)
                    .CountAsync();

                // Recent items
                vm.MaterielList = await _context.Materiels
                    .Include(m => m.Categorie)
                    .Include(m => m.Service)
                    .OrderByDescending(m => m.CreatedDate)
                    .Take(5)
                    .ToListAsync();

                vm.CategorieList = await _context.Categories
                    .OrderByDescending(c => c.Id)
                    .Take(5)
                    .ToListAsync();

                vm.ServiceList = await _context.Services
                    .OrderByDescending(s => s.Id)
                    .Take(5)
                    .ToListAsync();

                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data");

                // Handle specific exception types
                if (ex.Message.Contains("User not authenticated") || ex.Message.Contains("not found in database"))
                {
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    return RedirectToAction("Login", "Admin");
                }

                // For other errors, show a friendly error page
                return View("Error", new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    ErrorMessage = "An error occurred while loading the dashboard. Please try again later."
                });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Admin");
        }

        // Add a simple health check endpoint for debugging
        [AllowAnonymous]
        public IActionResult Health()
        {
            return Content("OK - HomeController is working");
        }
    }
}