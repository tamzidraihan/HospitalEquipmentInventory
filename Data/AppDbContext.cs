using InvWebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace InvWebApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Materiel> Materiels { get; set; }
        public DbSet<Categorie> Categories { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<LogList> LogLists { get; set; }
        public DbSet<ServiceGroup> serviceGroups { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<InventoryStock> InventoryStocks { get; set; }
        public DbSet<InventoryBatch> InventoryBatches { get; set; }
        public DbSet<InventoryMovement> InventoryMovements { get; set; }
        public DbSet<WorkOrder> WorkOrders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the composite primary key for the UserRole junction table
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            // Configure the relationships
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            // Seed default roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, RoleName = "Admin", Description = "System Administrator with full access" },
                new Role { Id = 2, RoleName = "Technician", Description = "Biomedical Technician who performs repairs" },
                new Role { Id = 3, RoleName = "Nurse", Description = "Hospital staff who can report issues" },
                new Role { Id = 4, RoleName = "InventoryManager", Description = "Manages equipment inventory and orders" }
            );

            // Seed the default admin user (WITHOUT the old Role property)
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    UserName = "admin",
                    Email = "admin@example.com",
                    Password = "$2a$11$T4MKZq8z4h57FhOfyTPnmuhL6MEG6VMP9BDZRoFZsx4hgdTw5XepW",
                    KeepLoggedIn = false
                }
            );

            // Assign the admin user to the Admin role
            modelBuilder.Entity<UserRole>().HasData(
                new UserRole { UserId = 1, RoleId = 1 }
            );

            // Your existing configurations...
            modelBuilder.Entity<InventoryStock>()
                .HasIndex(s => new { s.MaterielId, s.LocationId }).IsUnique();

            modelBuilder.Entity<InventoryBatch>()
                .HasOne(b => b.InventoryStock)
                .WithMany()
                .HasForeignKey(b => b.InventoryStockId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InventoryMovement>()
                .HasOne(m => m.FromLocation)
                .WithMany()
                .HasForeignKey(m => m.FromLocationId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<InventoryMovement>()
                .HasOne(m => m.ToLocation)
                .WithMany()
                .HasForeignKey(m => m.ToLocationId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}