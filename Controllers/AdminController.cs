using InvWebApp.Data;
using InvWebApp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InvWebApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            ClaimsPrincipal claimUser = HttpContext.User;
            if (claimUser.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(User UserLogin)
        {
            Console.WriteLine($"=== LOGIN ATTEMPT ===");
            Console.WriteLine($"Username: {UserLogin.UserName}");

            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserName == UserLogin.UserName);

            if (user != null)
            {
                Console.WriteLine($"User found: {user.UserName}, ID: {user.Id}");

                bool passwordValid = VerifyPassword(UserLogin.Password, user.Password);
                Console.WriteLine($"Password valid: {passwordValid}");

                if (passwordValid)
                {
                    Console.WriteLine("Creating claims...");
                    var claims = new List<Claim>
                    {
                        // FIXED: Use user.Id instead of user.UserName for NameIdentifier
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim(ClaimTypes.Email, user.Email ?? ""),
                        new Claim("UserId", user.Id.ToString()) // Additional claim for easy access
                    };

                    // Add all roles as claims
                    foreach (var userRole in user.UserRoles)
                    {
                        Console.WriteLine($"Adding role claim: {userRole.Role.RoleName}");
                        claims.Add(new Claim(ClaimTypes.Role, userRole.Role.RoleName));
                    }

                    Console.WriteLine("Creating claims identity...");
                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims,
                        CookieAuthenticationDefaults.AuthenticationScheme);

                    AuthenticationProperties properties = new AuthenticationProperties()
                    {
                        AllowRefresh = true,
                        IsPersistent = UserLogin.KeepLoggedIn
                    };

                    Console.WriteLine("Signing in...");
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity), properties);

                    Console.WriteLine("Redirecting to home...");
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    Console.WriteLine("Password invalid");
                }
            }
            else
            {
                Console.WriteLine("User not found");
            }

            ViewData["ValidateMessage"] = "Invalid username or password";
            return View();
        }

        // GET: Admin/Register
        public IActionResult Register()
        {
            ViewBag._class = "alert alert-danger d-none";
            return View();
        }

        // POST: Admin/Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            ViewBag._class = "alert alert-danger d-none";

            Console.WriteLine($"Register attempt: {model.UserName}, {model.Email}");

            // Check if username already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName.ToLower() == model.UserName.ToLower());

            if (existingUser != null)
            {
                Console.WriteLine("Username already exists");
                ModelState.AddModelError("UserName", "Username already exists.");
                ViewBag._class = "alert alert-danger d-block";
            }

            // Check if email already exists
            if (!string.IsNullOrEmpty(model.Email))
            {
                var existingEmail = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email != null && u.Email.ToLower() == model.Email.ToLower());

                if (existingEmail != null)
                {
                    Console.WriteLine("Email already exists");
                    ModelState.AddModelError("Email", "Email already registered.");
                    ViewBag._class = "alert alert-danger d-block";
                }
            }

            if (!ModelState.IsValid)
            {
                Console.WriteLine($"ModelState invalid. Errors: {string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))}");
                return View(model);
            }

            string hashedPassword = HashPassword(model.Password);
            Console.WriteLine($"Password hashed successfully");

            var newUser = new User
            {
                UserName = model.UserName,
                Email = model.Email,
                Password = hashedPassword,
                KeepLoggedIn = model.KeepLoggedIn,
                FullName = model.FullName, // Make sure to include FullName
                PhoneNumber = model.PhoneNumber // Make sure to include PhoneNumber
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            Console.WriteLine($"User saved with ID: {newUser.Id}");

            // Assign default role (Nurse) to the new user
            var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Nurse");
            if (defaultRole != null)
            {
                var userRole = new UserRole
                {
                    UserId = newUser.Id,
                    RoleId = defaultRole.Id
                };
                _context.UserRoles.Add(userRole);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Assigned Nurse role to user");
            }

            return RedirectToAction("Login", "Admin");
        }

        // Hashing Passwords using bcrypt
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // Validating Passwords against hashed password
        public bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }

        [HttpGet]
        public IActionResult TestHash()
        {
            string password = "admin123";
            string hash = BCrypt.Net.BCrypt.HashPassword(password);

            Console.WriteLine($"Password: {password}");
            Console.WriteLine($"Generated hash: {hash}");
            Console.WriteLine($"Verify test: {BCrypt.Net.BCrypt.Verify(password, hash)}");

            return Content($"Hash: {hash}<br>Verify: {BCrypt.Net.BCrypt.Verify(password, hash)}");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Admin");
        }
    }
}