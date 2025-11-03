using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThongKe.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEntity_2025_10_10_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "NhomDanhMuc",
                table: "KieuDanhMuc",
                type: "NVARCHAR2(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(2000)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ParentMaMuc",
                table: "DanhMuc",
                type: "NVARCHAR2(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(2000)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ParentMaKieu",
                table: "DanhMuc",
                type: "NVARCHAR2(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(2000)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MaMuc",
                table: "DanhMuc",
                type: "NVARCHAR2(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(2000)");

            migrationBuilder.AlterColumn<string>(
                name: "MaKieu",
                table: "DanhMuc",
                type: "NVARCHAR2(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(2000)");

            migrationBuilder.CreateIndex(
                name: "IX_KieuDanhMuc_NhomDanhMuc",
                table: "KieuDanhMuc",
                column: "NhomDanhMuc");

            migrationBuilder.CreateIndex(
                name: "IX_DanhMuc_MaKieu",
                table: "DanhMuc",
                column: "MaKieu");

            migrationBuilder.CreateIndex(
                name: "IX_DanhMuc_MaMuc",
                table: "DanhMuc",
                column: "MaMuc");

            migrationBuilder.CreateIndex(
                name: "IX_DanhMuc_ParentMaKieu",
                table: "DanhMuc",
                column: "ParentMaKieu");

            migrationBuilder.CreateIndex(
                name: "IX_DanhMuc_ParentMaMuc",
                table: "DanhMuc",
                column: "ParentMaMuc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_KieuDanhMuc_NhomDanhMuc",
                table: "KieuDanhMuc");

            migrationBuilder.DropIndex(
                name: "IX_DanhMuc_MaKieu",
                table: "DanhMuc");

            migrationBuilder.DropIndex(
                name: "IX_DanhMuc_MaMuc",
                table: "DanhMuc");

            migrationBuilder.DropIndex(
                name: "IX_DanhMuc_ParentMaKieu",
                table: "DanhMuc");

            migrationBuilder.DropIndex(
                name: "IX_DanhMuc_ParentMaMuc",
                table: "DanhMuc");

            migrationBuilder.AlterColumn<string>(
                name: "NhomDanhMuc",
                table: "KieuDanhMuc",
                type: "NVARCHAR2(2000)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ParentMaMuc",
                table: "DanhMuc",
                type: "NVARCHAR2(2000)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ParentMaKieu",
                table: "DanhMuc",
                type: "NVARCHAR2(2000)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MaMuc",
                table: "DanhMuc",
                type: "NVARCHAR2(2000)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(450)");

            migrationBuilder.AlterColumn<string>(
                name: "MaKieu",
                table: "DanhMuc",
                type: "NVARCHAR2(2000)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(450)");
        }
    }
}
