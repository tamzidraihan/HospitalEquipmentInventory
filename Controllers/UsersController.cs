using InvWebApp.Data;
using InvWebApp.Extentions;
using InvWebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InvWebApp.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (role == "Admin")
            {
                // Admin sees all users with their roles
                var users = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .ToListAsync();
                return View(users);
            }
            else
            {
                // Regular user only sees self
                var userId = User.getUserId(_context);
                User user = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Id == userId);
                if (user != null)
                {
                    return View(new List<User> { user });
                }
                return NotFound();
            }
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,UserName,Email,Password,KeepLoggedIn")] User user)
        {
            if (ModelState.IsValid)
            {
                user.Password = HashPassword(user.Password);
                _context.Add(user);
                await _context.SaveChangesAsync();

                // Assign default role (Nurse) to the new user
                var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Nurse");
                if (defaultRole != null)
                {
                    var userRole = new UserRole
                    {
                        UserId = user.Id,
                        RoleId = defaultRole.Id
                    };
                    _context.UserRoles.Add(userRole);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserName,Email,Password,KeepLoggedIn")] User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    user.Password = HashPassword(user.Password);
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Users == null)
            {
                return Problem("Entity set 'AppDbContext.Users'  is null.");
            }
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user != null)
            {
                // Remove user roles first to avoid foreign key constraint issues
                _context.UserRoles.RemoveRange(user.UserRoles);
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return (_context.Users?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageRoles()
        {
            var users = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ToListAsync();

            // Get all available roles
            var allRoles = await _context.Roles.ToListAsync();
            ViewBag.AllRoles = allRoles;

            return View(users);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRole(int userId, int roleId, bool isAssigned)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("ManageRoles");
            }

            var role = await _context.Roles.FindAsync(roleId);
            if (role == null)
            {
                TempData["ErrorMessage"] = "Role not found.";
                return RedirectToAction("ManageRoles");
            }

            var userRole = user.UserRoles.FirstOrDefault(ur => ur.RoleId == roleId);

            if (isAssigned)
            {
                if (userRole == null)
                {
                    user.UserRoles.Add(new UserRole { UserId = userId, RoleId = roleId });
                    TempData["SuccessMessage"] = $"Assigned '{role.RoleName}' role to {user.UserName}.";
                }
            }
            else
            {
                if (userRole != null)
                {
                    _context.UserRoles.Remove(userRole);
                    TempData["SuccessMessage"] = $"Removed '{role.RoleName}' role from {user.UserName}.";
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("ManageRoles");
        }

        public async Task<IActionResult> Profile()
        {
            // Get current user ID
            var userId = GetCurrentUserId();

            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            var model = new ProfileViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.UserRoles?.FirstOrDefault()?.Role?.RoleName ?? "No Role" // Changed from .Name to .RoleName
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FindAsync(model.Id);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{model.Id}'.");
            }

            // Update basic info
            user.UserName = model.UserName;
            user.FullName = model.FullName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;

            // Handle password change if provided
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                if (string.IsNullOrEmpty(model.OldPassword))
                {
                    ModelState.AddModelError("OldPassword", "Current password is required to set a new password.");
                    return View(model);
                }

                // Verify old password first
                if (!VerifyPassword(model.OldPassword, user.Password))
                {
                    ModelState.AddModelError("OldPassword", "Current password is incorrect.");
                    return View(model);
                }

                user.Password = HashPassword(model.NewPassword);
            }

            try
            {
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                TempData["StatusMessage"] = "Your profile has been updated";
                return RedirectToAction(nameof(Profile));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                return View(model);
            }
        }

        private int GetCurrentUserId()
        {
            // Method 1: Directly from NameIdentifier claim (using the actual URI)
            var nameIdentifierClaim = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            if (nameIdentifierClaim != null && int.TryParse(nameIdentifierClaim.Value, out int userId1))
            {
                return userId1;
            }

            // Method 2: Using ClaimTypes.NameIdentifier (the constant)
            var nameIdentifierClaim2 = User.FindFirst(ClaimTypes.NameIdentifier);
            if (nameIdentifierClaim2 != null && int.TryParse(nameIdentifierClaim2.Value, out int userId2))
            {
                return userId2;
            }

            // Method 3: From username
            var username = User.Identity.Name;
            if (!string.IsNullOrEmpty(username))
            {
                var user = _context.Users.FirstOrDefault(u => u.UserName == username);
                if (user != null)
                {
                    return user.Id;
                }
            }

            // Method 4: From email claim
            var emailClaim = User.FindFirst(ClaimTypes.Email);
            if (emailClaim != null)
            {
                var user = _context.Users.FirstOrDefault(u => u.Email == emailClaim.Value);
                if (user != null)
                {
                    return user.Id;
                }
            }

            throw new Exception($"User not authenticated. Available claims: {string.Join("; ", User.Claims.Select(c => $"{c.Type}={c.Value}"))}");
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }

        public IActionResult DebugClaims()
        {
            var claims = User.Claims.Select(c => new { Type = c.Type, Value = c.Value }).ToList();
            return Json(claims);
        }
    }
}