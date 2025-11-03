using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThongKe.Migrations
{
    /// <inheritdoc />
    public partial class addpermissionv3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RolePermission");

            migrationBuilder.AddColumn<int>(
                name: "IsGranted",
                table: "Permission",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RoleId",
                table: "Permission",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CongBoDienTichCayLauNamChinhs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    MaTinhThanh = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    TenTinhThanh = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    LoaiCay = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    MuaVu = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Nam = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Thang = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DienTichThuHoach = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    DienTichGieoTrong = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    DienTichUocTinhGieoTrong = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    DienTichUocTinhThuHoach = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    DonViTinh = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    IsCongBo = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    CreatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CongBoDienTichCayLauNamChinhs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CongBoNangSuatLuas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    MaTinhThanh = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    TenTinhThanh = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    MuaVu = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Nam = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Thang = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    NangSuatLua = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    NangSuatLuaUocTinh = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    DonViTinh = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    IsCongBo = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    CreatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CongBoNangSuatLuas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CongBoSanLuongCayHangNamKhacs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    MaTinhThanh = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    TenTinhThanh = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    LoaiCay = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    MuaVu = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Nam = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Thang = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    SanLuong = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    SanLuongUocTinh = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    DonViTinh = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    IsCongBo = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    CreatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CongBoSanLuongCayHangNamKhacs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CongBoSanLuongCayLauNamChinhs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    MaTinhThanh = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    TenTinhThanh = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    LoaiCay = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    MuaVu = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Nam = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Thang = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    SanLuong = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    SanLuongUocTinh = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    DonViTinh = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    IsCongBo = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    CreatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CongBoSanLuongCayLauNamChinhs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CongBoSanLuongLuas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    MaTinhThanh = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    TenTinhThanh = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    MuaVu = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Nam = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Thang = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    SanLuongLua = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    SanLuongLuaUocTinh = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    DonViTinh = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    IsCongBo = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    CreatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CongBoSanLuongLuas", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CongBoDienTichCayLauNamChinhs");

            migrationBuilder.DropTable(
                name: "CongBoNangSuatLuas");

            migrationBuilder.DropTable(
                name: "CongBoSanLuongCayHangNamKhacs");

            migrationBuilder.DropTable(
                name: "CongBoSanLuongCayLauNamChinhs");

            migrationBuilder.DropTable(
                name: "CongBoSanLuongLuas");

            migrationBuilder.DropColumn(
                name: "IsGranted",
                table: "Permission");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "Permission");

            migrationBuilder.CreateTable(
                name: "RolePermission",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    IsGranted = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    PermissionId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    RoleId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermission", x => x.Id);
                });
        }
    }
}
