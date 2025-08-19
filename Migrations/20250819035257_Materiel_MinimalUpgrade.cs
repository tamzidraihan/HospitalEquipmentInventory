using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvWebApp.Migrations
{
    /// <inheritdoc />
    public partial class MaterielMinimalUpgrade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Materiels_Users_UserId",
                table: "Materiels");

            migrationBuilder.DropForeignKey(
                name: "FK_Materiels_serviceGroups_ServiceGroupId",
                table: "Materiels");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Materiels",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "ServiceGroupId",
                table: "Materiels",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "SerialNumber",
                table: "Materiels",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "AssetTag",
                table: "Materiels",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "Kind",
                table: "Materiels",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReorderPoint",
                table: "Materiels",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReorderQuantity",
                table: "Materiels",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Materiels",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitCost",
                table: "Materiels",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Uom",
                table: "Materiels",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_Materiels_Users_UserId",
                table: "Materiels",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Materiels_serviceGroups_ServiceGroupId",
                table: "Materiels",
                column: "ServiceGroupId",
                principalTable: "serviceGroups",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Materiels_Users_UserId",
                table: "Materiels");

            migrationBuilder.DropForeignKey(
                name: "FK_Materiels_serviceGroups_ServiceGroupId",
                table: "Materiels");

            migrationBuilder.DropColumn(
                name: "AssetTag",
                table: "Materiels");

            migrationBuilder.DropColumn(
                name: "Kind",
                table: "Materiels");

            migrationBuilder.DropColumn(
                name: "ReorderPoint",
                table: "Materiels");

            migrationBuilder.DropColumn(
                name: "ReorderQuantity",
                table: "Materiels");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Materiels");

            migrationBuilder.DropColumn(
                name: "UnitCost",
                table: "Materiels");

            migrationBuilder.DropColumn(
                name: "Uom",
                table: "Materiels");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Materiels",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ServiceGroupId",
                table: "Materiels",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Materiels",
                keyColumn: "SerialNumber",
                keyValue: null,
                column: "SerialNumber",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "SerialNumber",
                table: "Materiels",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_Materiels_Users_UserId",
                table: "Materiels",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Materiels_serviceGroups_ServiceGroupId",
                table: "Materiels",
                column: "ServiceGroupId",
                principalTable: "serviceGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
