using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThongKe.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEntity_2025_10_24 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PhienBanDanhMucId",
                table: "ChiTieus",
                newName: "ParentId");

            migrationBuilder.RenameIndex(
                name: "IX_ChiTieus_PhienBanDanhMucId",
                table: "ChiTieus",
                newName: "IX_ChiTieus_ParentId");

            migrationBuilder.AddColumn<string>(
                name: "ColumnName",
                table: "ChiTieus",
                type: "NVARCHAR2(2000)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsBienThe",
                table: "ChiTieus",
                type: "NUMBER(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TableName",
                table: "ChiTieus",
                type: "NVARCHAR2(2000)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TrangThai",
                table: "ChiTieus",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ChiTieus_TrangThai",
                table: "ChiTieus",
                column: "TrangThai");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChiTieus_TrangThai",
                table: "ChiTieus");

            migrationBuilder.DropColumn(
                name: "ColumnName",
                table: "ChiTieus");

            migrationBuilder.DropColumn(
                name: "IsBienThe",
                table: "ChiTieus");

            migrationBuilder.DropColumn(
                name: "TableName",
                table: "ChiTieus");

            migrationBuilder.DropColumn(
                name: "TrangThai",
                table: "ChiTieus");

            migrationBuilder.RenameColumn(
                name: "ParentId",
                table: "ChiTieus",
                newName: "PhienBanDanhMucId");

            migrationBuilder.RenameIndex(
                name: "IX_ChiTieus_ParentId",
                table: "ChiTieus",
                newName: "IX_ChiTieus_PhienBanDanhMucId");

            migrationBuilder.CreateTable(
                name: "PhienBanChiTieus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    ChiTieusJSON = table.Column<string>(type: "CLOB", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    CreatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    DonViThongKeId = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ExpiredAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    PhienBanDanhMucId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    Scope = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    TenPhienBan = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true),
                    TinhThanhId = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    TrangThai = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UpdatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhienBanChiTieus", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PhienBanChiTieus_CreatedBy",
                table: "PhienBanChiTieus",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PhienBanChiTieus_PhienBanDanhMucId",
                table: "PhienBanChiTieus",
                column: "PhienBanDanhMucId");

            migrationBuilder.CreateIndex(
                name: "IX_PhienBanChiTieus_TrangThai",
                table: "PhienBanChiTieus",
                column: "TrangThai");
        }
    }
}
