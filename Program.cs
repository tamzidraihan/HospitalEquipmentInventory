using InvWebApp.Data;
using InvWebApp.Models;
using InvWebApp.Services.Inventory;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// MVC + Services
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IInventoryService, InventoryService>();

// Cookie auth
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opt =>
    {
        opt.LoginPath = "/Admin/Login";
        opt.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    });

// DbContext (MySQL via Pomelo)
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
    options.EnableDetailedErrors();
    options.EnableSensitiveDataLogging();
});

// Add session services
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// ---- MIDDLEWARE PIPELINE ----
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

// ---- DATABASE SEEDING (Moved after middleware but before endpoints) ----
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    try
    {
        // Ensure database is created and migrated
        context.Database.EnsureCreated();

        // Ensure roles exist first
        if (!context.Roles.Any())
        {
            context.Roles.AddRange(
                new Role { RoleName = "Admin" },
                new Role { RoleName = "Nurse" },
                new Role { RoleName = "Technician" }
            );
            context.SaveChanges();
        }

        // Ensure admin user exists
        var adminUser = context.Users.FirstOrDefault(u => u.UserName == "admin");
        if (adminUser == null)
        {
            // Create admin user if it doesn't exist
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("admin123");
            var user = new User
            {
                UserName = "admin",
                Email = "admin@example.com",
                Password = hashedPassword,
                FullName = "Administrator",
                PhoneNumber = "+1234567890"
            };
            context.Users.Add(user);
            context.SaveChanges();

            // Assign admin role - ensure the role exists
            var adminRole = context.Roles.FirstOrDefault(r => r.RoleName == "Admin");
            if (adminRole != null)
            {
                context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = adminRole.Id });
                context.SaveChanges();
            }
        }

        // Seed Locations
        if (!context.Locations.Any())
        {
            context.Locations.AddRange(
                new Location { Name = "Main Store" },
                new Location { Name = "ICU Store" }
            );
            context.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        // Log the error but don't crash the application
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "login",
    pattern: "auth",
    defaults: new { controller = "Admin", action = "Login" });

app.Run();