using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThongKe.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEntity_2025_10_24_6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChiTieus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    TenChiTieu = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: false),
                    Hash = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    TrangThai = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ParentId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    IsBienThe = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    MoTa = table.Column<string>(type: "NVARCHAR2(2000)", maxLength: 2000, nullable: true),
                    DonViId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    PhanTosJSON = table.Column<string>(type: "CLOB", nullable: false),
                    ColumnName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    TableName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UpdatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    UpdatedBy2 = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    Scope = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    TinhThanhId = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DonViThongKeId = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    CreatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTieus", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChiTieus_CreatedBy",
                table: "ChiTieus",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTieus_DonViId",
                table: "ChiTieus",
                column: "DonViId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTieus_ParentId",
                table: "ChiTieus",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTieus_TenChiTieu",
                table: "ChiTieus",
                column: "TenChiTieu",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChiTieus_TrangThai",
                table: "ChiTieus",
                column: "TrangThai");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTieus_UpdatedBy",
                table: "ChiTieus",
                column: "UpdatedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiTieus");
        }
    }
}
