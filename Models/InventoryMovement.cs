using System;

namespace InvWebApp.Models
{
    public class InventoryMovement
    {
        public int Id { get; set; }
        public int MaterielId { get; set; }
        public Materiel Materiel { get; set; } = default!;

        public int? FromLocationId { get; set; }
        public Location? FromLocation { get; set; }

        public int? ToLocationId { get; set; }
        public Location? ToLocation { get; set; }

        public int? BatchId { get; set; }
        public InventoryBatch? Batch { get; set; }

        public MovementType Type { get; set; }
        public int Quantity { get; set; }
        public string? Reason { get; set; }
        public string? PerformedByUserName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
