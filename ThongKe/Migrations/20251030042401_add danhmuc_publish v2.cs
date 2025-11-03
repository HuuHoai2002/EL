using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThongKe.Migrations
{
    /// <inheritdoc />
    public partial class adddanhmuc_publishv2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DanhMucMoRongsJSON",
                table: "KieuDanhMuc");

            migrationBuilder.RenameColumn(
                name: "CayDanhMucJson",
                table: "DanhMucPublish",
                newName: "DanhMucsJson");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DanhMucsJson",
                table: "DanhMucPublish",
                newName: "CayDanhMucJson");

            migrationBuilder.AddColumn<string>(
                name: "DanhMucMoRongsJSON",
                table: "KieuDanhMuc",
                type: "NCLOB",
                nullable: true);
        }
    }
}
