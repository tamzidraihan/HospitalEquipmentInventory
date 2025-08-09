using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvWebApp.Models
{
    public class Materiel
    {
        public int Id { get; set; }
        public string MaterielName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string SerialNumber { get; set; }
        public string? InventoryNumber { get; set; }
        public string? MaterielOwner { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        public int Quantity { get; set; } 
        public int CategorieId { get; set; }
        public int ServiceId { get; set; }
        public int ServiceGroupId { get; set; }
        public int UserId { get; set; }

        [ForeignKey("CategorieId")]
        public Categorie? Categorie { get; set; }

        [ForeignKey("ServiceId")]
        public Service? Service { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [ForeignKey("ServiceGroupId")]
        public ServiceGroup? serviceGroup { get; set; }
    }

}