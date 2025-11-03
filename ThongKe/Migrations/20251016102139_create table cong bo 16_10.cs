using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThongKe.Migrations
{
    /// <inheritdoc />
    public partial class createtablecongbo16_10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CongBoDienTichNuoiTrongThuySans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    MaTinhThanh = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    TenTinhThanh = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    LoaiThuySanChinh = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    MoiTruongNuoi = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Nam = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Thang = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DienTich = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    DienTichUocTinh = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    DonViTinh = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    IsCongBo = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    CreatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CongBoDienTichNuoiTrongThuySans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CongBoSanLuongNuoiTrongThuySans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    MaTinhThanh = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    TenTinhThanh = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    LoaiThuySanChinh = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    MoiTruongNuoi = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
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
                    table.PrimaryKey("PK_CongBoSanLuongNuoiTrongThuySans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CongBoSanLuongThuySanKhaiThacs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    MaTinhThanh = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    TenTinhThanh = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    LoaiThuySanChinh = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
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
                    table.PrimaryKey("PK_CongBoSanLuongThuySanKhaiThacs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CongBoSoLuongGiaSucGiaCams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    MaTinhThanh = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    TenTinhThanh = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    LoaiVatNuoi = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Nam = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Thang = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    SoLuong = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    SoLuongUocTinh = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    DonViTinh = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    IsCongBo = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    CreatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CongBoSoLuongGiaSucGiaCams", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CongBoDienTichNuoiTrongThuySans");

            migrationBuilder.DropTable(
                name: "CongBoSanLuongNuoiTrongThuySans");

            migrationBuilder.DropTable(
                name: "CongBoSanLuongThuySanKhaiThacs");

            migrationBuilder.DropTable(
                name: "CongBoSoLuongGiaSucGiaCams");
        }
    }
}
