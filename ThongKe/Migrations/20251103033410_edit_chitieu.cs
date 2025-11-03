using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThongKe.Migrations
{
    /// <inheritdoc />
    public partial class edit_chitieu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BienThesJSON",
                table: "ChiTieus",
                type: "CLOB",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ma",
                table: "ChiTieus",
                type: "NVARCHAR2(2000)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaTrangThai",
                table: "ChiTieus",
                type: "NUMBER(10)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ten",
                table: "ChiTieus",
                type: "NVARCHAR2(2000)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BienThesJSON",
                table: "ChiTieus");

            migrationBuilder.DropColumn(
                name: "Ma",
                table: "ChiTieus");

            migrationBuilder.DropColumn(
                name: "MaTrangThai",
                table: "ChiTieus");

            migrationBuilder.DropColumn(
                name: "Ten",
                table: "ChiTieus");
        }
    }
}
