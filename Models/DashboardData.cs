using System.Collections.Generic;

namespace InvWebApp.Models
{
    public class DashboardData
    {
        // Existing properties
        public int MaterielCount { get; set; }
        public int CategoryCount { get; set; }
        public int ServiceCount { get; set; }
        public int UserCount { get; set; }
        public int LowStockCount { get; set; }
        public int OpenWorkOrdersCount { get; set; }
        public int OperationalEquipmentCount { get; set; }

        public List<Materiel> MaterielList { get; set; }
        public List<Categorie> CategorieList { get; set; }
        public List<Service> ServiceList { get; set; }
        public List<LowStockItem> LowStockTop { get; set; }
        public List<WorkOrder> WorkOrdersTop { get; set; }
        public List<WorkOrder> MyAssignedWorkOrders { get; set; }
        public List<LogList> _LogList { get; set; }

        // Add these classes if needed
        public class LowStockItem
        {
            public string MaterielName { get; set; }
            public int Quantity { get; set; }
            public int? ReorderPoint { get; set; }
            public string Location { get; set; }
        }
    }
}
