using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvWebApp.Models
{
    public class WorkOrder
    {
        public int Id { get; set; }

        [Display(Name = "Materiel")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a materiel.")]
        public int MaterielId { get; set; }

        [ForeignKey(nameof(MaterielId))]
        [ValidateNever]                 // <— important: don’t bind this from the form
        public Materiel Materiel { get; set; } = default!;

        [Required, StringLength(100)]
        public string Title { get; set; } = default!;

        [StringLength(500)]
        public string? Description { get; set; }

        public WorkOrderPriority Priority { get; set; } = WorkOrderPriority.Medium;
        public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Open;

        public DateTime OpenDate { get; set; } = DateTime.UtcNow;
        public DateTime? DueDate { get; set; }

        public string? AssignedTo { get; set; } // simple text for demo
        public DateTime? CompletedDate { get; set; }
        public string? ResolutionNote { get; set; }
    }
}
