using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvWebApp.Migrations
{
    /// <inheritdoc />
    public partial class FixUserRoleData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Check if user with ID 1 exists before adding the role
            migrationBuilder.Sql(@"
        INSERT INTO UserRoles (UserId, RoleId)
        SELECT 1, 1
        FROM Users
        WHERE Users.Id = 1
        AND NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = 1 AND RoleId = 1)
    ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
