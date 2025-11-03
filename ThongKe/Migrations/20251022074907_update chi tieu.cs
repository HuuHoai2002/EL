using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThongKe.Migrations
{
    /// <inheritdoc />
    public partial class updatechitieu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "DanhMucImport",
                type: "NVARCHAR2(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(2000)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DonViThongKeId",
                table: "DanhMucImport",
                type: "NVARCHAR2(2000)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Scope",
                table: "DanhMucImport",
                type: "NVARCHAR2(2000)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TinhThanhId",
                table: "DanhMucImport",
                type: "NVARCHAR2(2000)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DonViThongKeId",
                table: "ChiTieus",
                type: "NVARCHAR2(2000)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Scope",
                table: "ChiTieus",
                type: "NVARCHAR2(2000)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TinhThanhId",
                table: "ChiTieus",
                type: "NVARCHAR2(2000)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DonViThongKeId",
                table: "ChiTieuDaNhaps",
                type: "NVARCHAR2(2000)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Scope",
                table: "ChiTieuDaNhaps",
                type: "NVARCHAR2(2000)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TinhThanhId",
                table: "ChiTieuDaNhaps",
                type: "NVARCHAR2(2000)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DonViThongKeId",
                table: "DanhMucImport");

            migrationBuilder.DropColumn(
                name: "Scope",
                table: "DanhMucImport");

            migrationBuilder.DropColumn(
                name: "TinhThanhId",
                table: "DanhMucImport");

            migrationBuilder.DropColumn(
                name: "DonViThongKeId",
                table: "ChiTieus");

            migrationBuilder.DropColumn(
                name: "Scope",
                table: "ChiTieus");

            migrationBuilder.DropColumn(
                name: "TinhThanhId",
                table: "ChiTieus");

            migrationBuilder.DropColumn(
                name: "DonViThongKeId",
                table: "ChiTieuDaNhaps");

            migrationBuilder.DropColumn(
                name: "Scope",
                table: "ChiTieuDaNhaps");

            migrationBuilder.DropColumn(
                name: "TinhThanhId",
                table: "ChiTieuDaNhaps");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "DanhMucImport",
                type: "NVARCHAR2(2000)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(100)",
                oldMaxLength: 100,
                oldNullable: true);
        }
    }
}
