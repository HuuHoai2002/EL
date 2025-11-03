using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThongKe.Migrations
{
    /// <inheritdoc />
    public partial class adddanhmucpublish : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MaKieuDanhMucs",
                table: "DanhMucPublish",
                newName: "MaKieuDanhMucGoc");

            migrationBuilder.AddColumn<string>(
                name: "ChildrenKieuDanhMucsJson",
                table: "DanhMucPublish",
                type: "NCLOB",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChildrenKieuDanhMucsJson",
                table: "DanhMucPublish");

            migrationBuilder.RenameColumn(
                name: "MaKieuDanhMucGoc",
                table: "DanhMucPublish",
                newName: "MaKieuDanhMucs");
        }
    }
}
