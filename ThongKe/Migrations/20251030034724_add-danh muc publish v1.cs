using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThongKe.Migrations
{
    /// <inheritdoc />
    public partial class adddanhmucpublishv1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChildrenKieuDanhMucsJson",
                table: "DanhMucPublish");

            migrationBuilder.DropColumn(
                name: "DanhMucMoRongsJSON",
                table: "DanhMucPublish");

            migrationBuilder.RenameColumn(
                name: "DanhMucsJSON",
                table: "DanhMucPublish",
                newName: "CayDanhMucJson");

            migrationBuilder.AddColumn<string>(
                name: "DanhMucMoRongsJSON",
                table: "KieuDanhMuc",
                type: "NCLOB",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DanhMucMoRongsJSON",
                table: "KieuDanhMuc");

            migrationBuilder.RenameColumn(
                name: "CayDanhMucJson",
                table: "DanhMucPublish",
                newName: "DanhMucsJSON");

            migrationBuilder.AddColumn<string>(
                name: "ChildrenKieuDanhMucsJson",
                table: "DanhMucPublish",
                type: "NCLOB",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DanhMucMoRongsJSON",
                table: "DanhMucPublish",
                type: "NCLOB",
                nullable: true);
        }
    }
}
