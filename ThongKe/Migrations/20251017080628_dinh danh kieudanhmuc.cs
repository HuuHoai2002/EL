using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThongKe.Migrations
{
    /// <inheritdoc />
    public partial class dinhdanhkieudanhmuc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DonViThongKeId",
                table: "KieuDanhMuc",
                type: "NVARCHAR2(2000)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Scope",
                table: "KieuDanhMuc",
                type: "NVARCHAR2(2000)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TinhThanhId",
                table: "KieuDanhMuc",
                type: "NVARCHAR2(2000)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DonViThongKeId",
                table: "KieuDanhMuc");

            migrationBuilder.DropColumn(
                name: "Scope",
                table: "KieuDanhMuc");

            migrationBuilder.DropColumn(
                name: "TinhThanhId",
                table: "KieuDanhMuc");
        }
    }
}
