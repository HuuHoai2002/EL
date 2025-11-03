using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThongKe.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEntity_2025_10_24_7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedBy2",
                table: "ChiTieus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy2",
                table: "ChiTieus",
                type: "NVARCHAR2(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
