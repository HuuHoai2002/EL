using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThongKe.Migrations
{
    /// <inheritdoc />
    public partial class adddanhmuc_publishv4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MaKieuDanhMucGoc",
                table: "DanhMucPublish",
                newName: "MaKieuDanhMucGocs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MaKieuDanhMucGocs",
                table: "DanhMucPublish",
                newName: "MaKieuDanhMucGoc");
        }
    }
}
