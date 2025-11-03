using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThongKe.Migrations
{
    /// <inheritdoc />
    public partial class addtablecongboxuatkhau : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CongBoXuatKhauDauVaoSanXuats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Nam = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Thang = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DonViGiaTri = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    GiaTri = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    DonViGiaTriUocTinh = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    GiaTriUocTinh = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    IsCongBo = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    CreatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CongBoXuatKhauDauVaoSanXuats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CongBoXuatKhauLamSans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    MaQuocGiaVungLT = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    TenQuocGiaVungLT = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Nam = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Thang = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    LoaiLamSan = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    DonViGiaTri = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    GiaTri = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    DonViGiaTriUocTinh = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    GiaTriUocTinh = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    IsCongBo = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    CreatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CongBoXuatKhauLamSans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CongBoXuatKhauMuois",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    MaQuocGiaVungLT = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    TenQuocGiaVungLT = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Nam = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Thang = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DonViGiaTri = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    GiaTri = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    DonViGiaTriUocTinh = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    GiaTriUocTinh = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    IsCongBo = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    CreatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CongBoXuatKhauMuois", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CongBoXuatKhauSanPhamChanNuois",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    MaQuocGiaVungLT = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    TenQuocGiaVungLT = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Nam = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Thang = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    LoaiSanPhamChanNuoi = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    DonViGiaTri = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    GiaTri = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    DonViGiaTriUocTinh = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    GiaTriUocTinh = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    IsCongBo = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    CreatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CongBoXuatKhauSanPhamChanNuois", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CongBoXuatKhauThuySans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    MaQuocGiaVungLT = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    TenQuocGiaVungLT = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Nam = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Thang = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DonViGiaTri = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    GiaTri = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    DonViGiaTriUocTinh = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    GiaTriUocTinh = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    IsCongBo = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    CreatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CongBoXuatKhauThuySans", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CongBoXuatKhauDauVaoSanXuats");

            migrationBuilder.DropTable(
                name: "CongBoXuatKhauLamSans");

            migrationBuilder.DropTable(
                name: "CongBoXuatKhauMuois");

            migrationBuilder.DropTable(
                name: "CongBoXuatKhauSanPhamChanNuois");

            migrationBuilder.DropTable(
                name: "CongBoXuatKhauThuySans");
        }
    }
}
