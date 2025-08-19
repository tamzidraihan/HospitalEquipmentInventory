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

var app = builder.Build(); 

// ---- Seed a couple of Locations ----
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (!db.Locations.Any())
    {
        db.Locations.AddRange(
            new Location { Name = "Main Store" },
            new Location { Name = "ICU Store" }
        );
        db.SaveChanges();
    }
}

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Routes — pick ONE default. Suggest Home/Index as default.
// The LoginPath will redirect unauthenticated users to /Admin/Login automatically.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// If you really want a route to Admin/Login as well, give it a different name:
app.MapControllerRoute(
    name: "login",
    pattern: "auth",
    defaults: new { controller = "Admin", action = "Login" });

app.Run();
