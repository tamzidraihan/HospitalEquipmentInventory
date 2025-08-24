using InvWebApp.Data;
using InvWebApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InvWebApp.Extentions
{
    public static class UserExtention
    {
        public static int getUserId(this ClaimsPrincipal user, AppDbContext _context)
        {
            // Try to get user ID from claims first (more efficient)
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) ??
                             user.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }

            // Fallback: Get from database using username
            var username = user.Identity.Name;

            if (string.IsNullOrEmpty(username))
            {
                throw new Exception("User not authenticated");
            }

            var currentUser = _context.Users
                .FirstOrDefault(u => u.UserName == username);

            if (currentUser == null)
            {
                throw new Exception($"User with username '{username}' not found in database");
            }

            return currentUser.Id;
        }
    }
}