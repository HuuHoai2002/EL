using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThongKe.Migrations
{
    /// <inheritdoc />
    public partial class createtableCongBoGiaSanPhamNLTSChinh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CongBoGiaSanPhamNLTSChinhs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    MaTinhThanh = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    TenTinhThanh = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    SanPhamNLTS = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    LoaiGia = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Nam = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Thang = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    GiaTri = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    GiaTriUocTinh = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    DonViTinh = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    IsCongBo = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    CreatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CongBoGiaSanPhamNLTSChinhs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CongBoGiaSanPhamNLTSChinhs");
        }
    }
}
