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

            // Optional: Seed default roles/users for testing
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    UserName = "admin",
                    Email = "admin@example.com",
                    Password = "$2a$11$T4MKZq8z4h57FhOfyTPnmuhL6MEG6VMP9BDZRoFZsx4hgdTw5XepW", // hashed 'admin123'
                    KeepLoggedIn = false,
                    Role = "Admin"
                }
            );

            modelBuilder.Entity<InventoryStock>()
                .HasIndex(s => new { s.MaterielId, s.LocationId }).IsUnique();

            modelBuilder.Entity<InventoryBatch>()
                .HasOne(b => b.InventoryStock).WithMany().HasForeignKey(b => b.InventoryStockId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InventoryMovement>()
                .HasOne(m => m.FromLocation).WithMany().HasForeignKey(m => m.FromLocationId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<InventoryMovement>()
                .HasOne(m => m.ToLocation).WithMany().HasForeignKey(m => m.ToLocationId)
                .OnDelete(DeleteBehavior.NoAction);

        }
    }
}