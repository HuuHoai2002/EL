using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThongKe.Migrations
{
    /// <inheritdoc />
    public partial class addmakieuv1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Ma",
                table: "KieuDanhMuc",
                newName: "MaKieu");

            migrationBuilder.RenameIndex(
                name: "IX_KieuDanhMuc_Ma",
                table: "KieuDanhMuc",
                newName: "IX_KieuDanhMuc_MaKieu");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MaKieu",
                table: "KieuDanhMuc",
                newName: "Ma");

            migrationBuilder.RenameIndex(
                name: "IX_KieuDanhMuc_MaKieu",
                table: "KieuDanhMuc",
                newName: "IX_KieuDanhMuc_Ma");
        }
    }
}
