using System.Collections.Generic;

namespace InvWebApp.Models
{
    public class DashboardData
    {
        // Topline counts
        public int MaterielCount { get; set; }
        public int CategoryCount { get; set; }
        public int ServiceCount { get; set; }
        public int UserCount { get; set; }
        public int LowStockCount { get; set; }
        public int OpenWorkOrdersCount { get; set; }

        // Lists shown on cards/tables
        public List<Materiel> MaterielList { get; set; } = new();
        public List<Categorie> CategorieList { get; set; } = new();
        public List<Service> ServiceList { get; set; } = new();
        public List<LogList> _LogList { get; set; } = new();

        // New sections
        public List<Materiel> LowStockTop { get; set; } = new();
        public List<WorkOrder> WorkOrdersTop { get; set; } = new();
    }
}
