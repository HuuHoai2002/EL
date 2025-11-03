using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThongKe.Migrations
{
    /// <inheritdoc />
    public partial class addrolev1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Role",
                newName: "NormalizedName");

            migrationBuilder.AddColumn<int>(
                name: "IsLocked",
                table: "Role",
                type: "NUMBER(10)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IsOpen",
                table: "Role",
                type: "NUMBER(10)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IsStatic",
                table: "Role",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "Updated",
                table: "Role",
                type: "NUMBER(19)",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsLocked",
                table: "Role");

            migrationBuilder.DropColumn(
                name: "IsOpen",
                table: "Role");

            migrationBuilder.DropColumn(
                name: "IsStatic",
                table: "Role");

            migrationBuilder.DropColumn(
                name: "Updated",
                table: "Role");

            migrationBuilder.RenameColumn(
                name: "NormalizedName",
                table: "Role",
                newName: "Status");
        }
    }
}
