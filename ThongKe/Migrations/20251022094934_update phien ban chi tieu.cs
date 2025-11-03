using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThongKe.Migrations
{
    /// <inheritdoc />
    public partial class updatephienbanchitieu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DonViThongKeId",
                table: "PhienBanChiTieus",
                type: "NVARCHAR2(2000)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Scope",
                table: "PhienBanChiTieus",
                type: "NVARCHAR2(2000)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TinhThanhId",
                table: "PhienBanChiTieus",
                type: "NVARCHAR2(2000)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DonViThongKeId",
                table: "DuLieuChiTieus",
                type: "NVARCHAR2(2000)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Scope",
                table: "DuLieuChiTieus",
                type: "NVARCHAR2(2000)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TinhThanhId",
                table: "DuLieuChiTieus",
                type: "NVARCHAR2(2000)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DonViThongKeId",
                table: "PhienBanChiTieus");

            migrationBuilder.DropColumn(
                name: "Scope",
                table: "PhienBanChiTieus");

            migrationBuilder.DropColumn(
                name: "TinhThanhId",
                table: "PhienBanChiTieus");

            migrationBuilder.DropColumn(
                name: "DonViThongKeId",
                table: "DuLieuChiTieus");

            migrationBuilder.DropColumn(
                name: "Scope",
                table: "DuLieuChiTieus");

            migrationBuilder.DropColumn(
                name: "TinhThanhId",
                table: "DuLieuChiTieus");
        }
    }
}
