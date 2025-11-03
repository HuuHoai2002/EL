using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThongKe.Migrations
{
    /// <inheritdoc />
    public partial class addkieudanhmucv1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DanhMuc");

            migrationBuilder.DropColumn(
                name: "ChildrenMas",
                table: "KieuDanhMuc");

            migrationBuilder.DropColumn(
                name: "ParentMa",
                table: "KieuDanhMuc");

            migrationBuilder.AddColumn<string>(
                name: "ChildrenKieuDanhMucsJson",
                table: "KieuDanhMuc",
                type: "NCLOB",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DanhMucsJson",
                table: "KieuDanhMuc",
                type: "NCLOB",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChildrenKieuDanhMucsJson",
                table: "KieuDanhMuc");

            migrationBuilder.DropColumn(
                name: "DanhMucsJson",
                table: "KieuDanhMuc");

            migrationBuilder.AddColumn<string>(
                name: "ChildrenMas",
                table: "KieuDanhMuc",
                type: "NVARCHAR2(2000)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParentMa",
                table: "KieuDanhMuc",
                type: "NVARCHAR2(2000)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DanhMuc",
                columns: table => new
                {
                    Id = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    ChildrenMaKieu = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ChildrenMaMucs = table.Column<string>(type: "NCLOB", nullable: true),
                    Created = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    CreatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    DanhSachsJSON = table.Column<string>(type: "NCLOB", nullable: true),
                    DonViThongKeId = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    KyHieu = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    MaKieu = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    MaMuc = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    MoTa = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Nguon = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ParentMaKieu = table.Column<string>(type: "NVARCHAR2(450)", nullable: true),
                    ParentMaMuc = table.Column<string>(type: "NVARCHAR2(450)", nullable: true),
                    Scope = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Stt = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    TenMuc = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    TinhThanhId = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Updated = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    UpdatedBy = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhMuc", x => x.Id);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_DanhMuc_TenMuc",
                table: "DanhMuc",
                column: "TenMuc");
        }
    }
}
