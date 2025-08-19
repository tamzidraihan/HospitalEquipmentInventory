using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvWebApp.Models
{
    public class Materiel
    {
        public int Id { get; set; }

        [Required] public string MaterielName { get; set; } = default!;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public MaterielKind Kind { get; set; } = MaterielKind.Consumable;

        // Catalog
        public string? Uom { get; set; }              // piece/box/bottle
        public decimal? UnitCost { get; set; }

        // Reorder (used for alerts later)
        public int? ReorderPoint { get; set; }        // alert when on-hand <= this
        public int? ReorderQuantity { get; set; }     // suggest ordering this much

        // Equipment-only (safe to ignore for Consumables)
        public string? SerialNumber { get; set; }
        public string? AssetTag { get; set; }
        public EquipmentStatus? Status { get; set; } = EquipmentStatus.Active;

        
        public string? InventoryNumber { get; set; }
        public string? MaterielOwner { get; set; }
        public int Quantity { get; set; } // will be informational; real stock will live in Inventory tables

        // FKs
        public int CategorieId { get; set; }
        public int ServiceId { get; set; }
        public int? ServiceGroupId { get; set; }
        public int? UserId { get; set; }

        // Navs
        public Categorie? Categorie { get; set; }
        public Service? Service { get; set; }
        public ServiceGroup? serviceGroup { get; set; }
        public User? User { get; set; }

        // Service/maintenance tracking (optional, nullable)
        public DateTime? LastServiceDate { get; set; }
        public DateTime? NextServiceDate { get; set; }
    }

}