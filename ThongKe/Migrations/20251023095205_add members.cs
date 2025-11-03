using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThongKe.Migrations
{
    /// <inheritdoc />
    public partial class addmembers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SupervisorId",
                table: "User");

            migrationBuilder.AddColumn<string>(
                name: "MemberJson",
                table: "User",
                type: "NVARCHAR2(2000)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MemberJson",
                table: "User");

            migrationBuilder.AddColumn<long>(
                name: "SupervisorId",
                table: "User",
                type: "NUMBER(19)",
                nullable: true);
        }
    }
}
