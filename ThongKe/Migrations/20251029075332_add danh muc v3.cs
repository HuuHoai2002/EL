using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThongKe.Migrations
{
    /// <inheritdoc />
    public partial class adddanhmucv3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaKieuDanhMucs",
                table: "DanhMucImport");

            migrationBuilder.DropColumn(
                name: "ParentMaKieu",
                table: "DanhMucImport");

            migrationBuilder.DropColumn(
                name: "ParentMaMuc",
                table: "DanhMucImport");

            migrationBuilder.RenameColumn(
                name: "TrangThai",
                table: "DanhMucImport",
                newName: "MaKieuGoc");

            migrationBuilder.RenameColumn(
                name: "RowsType",
                table: "DanhMucImport",
                newName: "MaKieu");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MaKieuGoc",
                table: "DanhMucImport",
                newName: "TrangThai");

            migrationBuilder.RenameColumn(
                name: "MaKieu",
                table: "DanhMucImport",
                newName: "RowsType");

            migrationBuilder.AddColumn<string>(
                name: "MaKieuDanhMucs",
                table: "DanhMucImport",
                type: "NVARCHAR2(2000)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParentMaKieu",
                table: "DanhMucImport",
                type: "NVARCHAR2(2000)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParentMaMuc",
                table: "DanhMucImport",
                type: "NVARCHAR2(2000)",
                nullable: true);
        }
    }
}
