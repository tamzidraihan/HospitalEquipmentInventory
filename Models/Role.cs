using System.Collections.Generic;

namespace InvWebApp.Models
{
    public class Role
    {
        public int Id { get; set; }
        public string RoleName { get; set; }  // e.g., "Admin", "Technician"
        public string? Description { get; set; }

        // Navigation property for the many-to-many relationship
        public ICollection<UserRole> UserRoles { get; set; }
    }
}