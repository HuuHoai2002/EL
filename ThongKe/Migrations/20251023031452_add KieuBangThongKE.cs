using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThongKe.Migrations
{
    /// <inheritdoc />
    public partial class addKieuBangThongKE : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DonViThongKeId",
                table: "KieuBangThongKes",
                type: "NVARCHAR2(2000)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Scope",
                table: "KieuBangThongKes",
                type: "NVARCHAR2(2000)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TinhThanhId",
                table: "KieuBangThongKes",
                type: "NVARCHAR2(2000)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DonViThongKeId",
                table: "KieuBangThongKes");

            migrationBuilder.DropColumn(
                name: "Scope",
                table: "KieuBangThongKes");

            migrationBuilder.DropColumn(
                name: "TinhThanhId",
                table: "KieuBangThongKes");
        }
    }
}
