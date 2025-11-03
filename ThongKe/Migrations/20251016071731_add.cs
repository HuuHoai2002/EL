using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThongKe.Migrations
{
  /// <inheritdoc />
  public partial class add : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(
          name: "Permission");

      migrationBuilder.AddColumn<string>(
          name: "DonViThongKeId",
          table: "User",
          type: "NVARCHAR2(2000)",
          nullable: true);

      migrationBuilder.AddColumn<string>(
          name: "Scope",
          table: "User",
          type: "NVARCHAR2(2000)",
          nullable: true);

      migrationBuilder.AddColumn<string>(
          name: "TinhThanhId",
          table: "User",
          type: "NVARCHAR2(2000)",
          nullable: true);

      migrationBuilder.AddColumn<string>(
          name: "MaChuyenNganh",
          table: "Role",
          type: "NVARCHAR2(2000)",
          nullable: true);

      migrationBuilder.AddColumn<string>(
          name: "MaCuc",
          table: "Role",
          type: "NVARCHAR2(2000)",
          nullable: true);

      migrationBuilder.AddColumn<string>(
          name: "MaDonViThongKe",
          table: "Role",
          type: "NVARCHAR2(2000)",
          nullable: true);

      migrationBuilder.AddColumn<string>(
          name: "MaTinh",
          table: "Role",
          type: "NVARCHAR2(2000)",
          nullable: true);

      migrationBuilder.AddColumn<string>(
          name: "Scope",
          table: "Role",
          type: "NVARCHAR2(2000)",
          nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropColumn(
          name: "DonViThongKeId",
          table: "User");

      migrationBuilder.DropColumn(
          name: "Scope",
          table: "User");

      migrationBuilder.DropColumn(
          name: "TinhThanhId",
          table: "User");

      migrationBuilder.DropColumn(
          name: "MaChuyenNganh",
          table: "Role");

      migrationBuilder.DropColumn(
          name: "MaCuc",
          table: "Role");

      migrationBuilder.DropColumn(
          name: "MaDonViThongKe",
          table: "Role");

      migrationBuilder.DropColumn(
          name: "MaTinh",
          table: "Role");

      migrationBuilder.DropColumn(
          name: "Scope",
          table: "Role");

      migrationBuilder.CreateTable(
          name: "Permission",
          columns: table => new
          {
            Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                  .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
            Created = table.Column<long>(type: "NUMBER(19)", nullable: false),
            Description = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
            Discriminator = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
            DisplayName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
            IsEnabled = table.Column<int>(type: "NUMBER(10)", nullable: false),
            IsGranted = table.Column<int>(type: "NUMBER(10)", nullable: false),
            IsGrantedByDefault = table.Column<int>(type: "NUMBER(10)", nullable: false),
            Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
            ParentId = table.Column<int>(type: "NUMBER(10)", nullable: true),
            ParentName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
            RoleId = table.Column<int>(type: "NUMBER(10)", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_Permission", x => x.Id);
          });
    }
  }
}
