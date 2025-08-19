namespace InvWebApp.Models
{
    public class InventoryStock
    {
        public int Id { get; set; }
        public int MaterielId { get; set; }
        public Materiel Materiel { get; set; } = default!;
        public int LocationId { get; set; }
        public Location Location { get; set; } = default!;
        public int Quantity { get; set; }
    }
}
