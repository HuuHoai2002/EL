using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThongKe.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEntity_2025_10_13_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TenMuc",
                table: "DanhMuc",
                type: "NVARCHAR2(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(2000)");

            migrationBuilder.CreateIndex(
                name: "IX_DanhMuc_TenMuc",
                table: "DanhMuc",
                column: "TenMuc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DanhMuc_TenMuc",
                table: "DanhMuc");

            migrationBuilder.AlterColumn<string>(
                name: "TenMuc",
                table: "DanhMuc",
                type: "NVARCHAR2(2000)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(450)");
        }
    }
}
