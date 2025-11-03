using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThongKe.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEntity_2025_10_30_3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TinhThanhId",
                table: "User",
                type: "NVARCHAR2(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(2000)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Scope",
                table: "User",
                type: "NVARCHAR2(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(2000)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TinhThanhId",
                table: "KieuDanhMuc",
                type: "NVARCHAR2(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(2000)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Scope",
                table: "KieuDanhMuc",
                type: "NVARCHAR2(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(2000)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TinhThanhId",
                table: "KieuBangThongKes",
                type: "NVARCHAR2(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(2000)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Scope",
                table: "KieuBangThongKes",
                type: "NVARCHAR2(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(2000)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TinhThanhId",
                table: "DuLieuChiTieus",
                type: "NVARCHAR2(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(2000)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Scope",
                table: "DuLieuChiTieus",
                type: "NVARCHAR2(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(2000)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TinhThanhId",
                table: "DonViThongKe",
                type: "NVARCHAR2(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(2000)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Scope",
                table: "DonViThongKe",
                type: "NVARCHAR2(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(2000)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TinhThanhId",
                table: "DanhMucImport",
                type: "NVARCHAR2(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(2000)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Scope",
                table: "DanhMucImport",
                type: "NVARCHAR2(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(2000)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TinhThanhId",
                table: "ChiTieus",
                type: "NVARCHAR2(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(2000)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Scope",
                table: "ChiTieus",
                type: "NVARCHAR2(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(2000)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TinhThanhId",
                table: "ChiTieuDaNhaps",
                type: "NVARCHAR2(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(2000)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Scope",
                table: "ChiTieuDaNhaps",
                type: "NVARCHAR2(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(2000)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_Scope",
                table: "User",
                column: "Scope");

            migrationBuilder.CreateIndex(
                name: "IX_User_TinhThanhId",
                table: "User",
                column: "TinhThanhId");

            migrationBuilder.CreateIndex(
                name: "IX_KieuDanhMuc_Scope",
                table: "KieuDanhMuc",
                column: "Scope");

            migrationBuilder.CreateIndex(
                name: "IX_KieuDanhMuc_TinhThanhId",
                table: "KieuDanhMuc",
                column: "TinhThanhId");

            migrationBuilder.CreateIndex(
                name: "IX_KieuBangThongKes_Scope",
                table: "KieuBangThongKes",
                column: "Scope");

            migrationBuilder.CreateIndex(
                name: "IX_KieuBangThongKes_TinhThanhId",
                table: "KieuBangThongKes",
                column: "TinhThanhId");

            migrationBuilder.CreateIndex(
                name: "IX_DuLieuChiTieus_Scope",
                table: "DuLieuChiTieus",
                column: "Scope");

            migrationBuilder.CreateIndex(
                name: "IX_DuLieuChiTieus_TinhThanhId",
                table: "DuLieuChiTieus",
                column: "TinhThanhId");

            migrationBuilder.CreateIndex(
                name: "IX_DonViThongKe_Scope",
                table: "DonViThongKe",
                column: "Scope");

            migrationBuilder.CreateIndex(
                name: "IX_DonViThongKe_TinhThanhId",
                table: "DonViThongKe",
                column: "TinhThanhId");

            migrationBuilder.CreateIndex(
                name: "IX_DanhMucImport_Scope",
                table: "DanhMucImport",
                column: "Scope");

            migrationBuilder.CreateIndex(
                name: "IX_DanhMucImport_TinhThanhId",
                table: "DanhMucImport",
                column: "TinhThanhId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTieus_Scope",
                table: "ChiTieus",
                column: "Scope");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTieus_TinhThanhId",
                table: "ChiTieus",
                column: "TinhThanhId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTieuDaNhaps_Scope",
                table: "ChiTieuDaNhaps",
                column: "Scope");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTieuDaNhaps_TinhThanhId",
                table: "ChiTieuDaNhaps",
                column: "TinhThanhId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_User_Scope",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_User_TinhThanhId",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_KieuDanhMuc_Scope",
                table: "KieuDanhMuc");

            migrationBuilder.DropIndex(
                name: "IX_KieuDanhMuc_TinhThanhId",
                table: "KieuDanhMuc");

            migrationBuilder.DropIndex(
                name: "IX_KieuBangThongKes_Scope",
                table: "KieuBangThongKes");

            migrationBuilder.DropIndex(
                name: "IX_KieuBangThongKes_TinhThanhId",
                table: "KieuBangThongKes");

            migrationBuilder.DropIndex(
                name: "IX_DuLieuChiTieus_Scope",
                table: "DuLieuChiTieus");

            migrationBuilder.DropIndex(
                name: "IX_DuLieuChiTieus_TinhThanhId",
                table: "DuLieuChiTieus");

            migrationBuilder.DropIndex(
                name: "IX_DonViThongKe_Scope",
                table: "DonViThongKe");

            migrationBuilder.DropIndex(
                name: "IX_DonViThongKe_TinhThanhId",
                table: "DonViThongKe");

            migrationBuilder.DropIndex(
                name: "IX_DanhMucImport_Scope",
                table: "DanhMucImport");

            migrationBuilder.DropIndex(
                name: "IX_DanhMucImport_TinhThanhId",
                table: "DanhMucImport");

            migrationBuilder.DropIndex(
                name: "IX_ChiTieus_Scope",
                table: "ChiTieus");

            migrationBuilder.DropIndex(
                name: "IX_ChiTieus_TinhThanhId",
                table: "ChiTieus");

            migrationBuilder.DropIndex(
                name: "IX_ChiTieuDaNhaps_Scope",
                table: "ChiTieuDaNhaps");

            migrationBuilder.DropIndex(
                name: "IX_ChiTieuDaNhaps_TinhThanhId",
                table: "ChiTieuDaNhaps");

            migrationBuilder.AlterColumn<string>(
                name: "TinhThanhId",
                table: "User",
                type: "NVARCHAR2(2000)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Scope",
                table: "User",
                type: "NVARCHAR2(2000)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TinhThanhId",
                table: "KieuDanhMuc",
                type: "NVARCHAR2(2000)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Scope",
                table: "KieuDanhMuc",
                type: "NVARCHAR2(2000)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TinhThanhId",
                table: "KieuBangThongKes",
                type: "NVARCHAR2(2000)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Scope",
                table: "KieuBangThongKes",
                type: "NVARCHAR2(2000)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TinhThanhId",
                table: "DuLieuChiTieus",
                type: "NVARCHAR2(2000)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Scope",
                table: "DuLieuChiTieus",
                type: "NVARCHAR2(2000)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TinhThanhId",
                table: "DonViThongKe",
                type: "NVARCHAR2(2000)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Scope",
                table: "DonViThongKe",
                type: "NVARCHAR2(2000)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TinhThanhId",
                table: "DanhMucImport",
                type: "NVARCHAR2(2000)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Scope",
                table: "DanhMucImport",
                type: "NVARCHAR2(2000)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TinhThanhId",
                table: "ChiTieus",
                type: "NVARCHAR2(2000)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Scope",
                table: "ChiTieus",
                type: "NVARCHAR2(2000)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TinhThanhId",
                table: "ChiTieuDaNhaps",
                type: "NVARCHAR2(2000)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Scope",
                table: "ChiTieuDaNhaps",
                type: "NVARCHAR2(2000)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(450)",
                oldNullable: true);
        }
    }
}
