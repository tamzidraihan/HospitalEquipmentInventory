using System;

namespace InvWebApp.Models
{
    public class InventoryBatch
    {
        public int Id { get; set; }
        public int InventoryStockId { get; set; }
        public InventoryStock InventoryStock { get; set; } = default!;

        public string? BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int Quantity { get; set; }
    }
}
