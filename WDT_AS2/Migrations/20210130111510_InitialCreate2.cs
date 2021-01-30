using Microsoft.EntityFrameworkCore.Migrations;

namespace WDT_AS2.Migrations
{
    public partial class InitialCreate2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountStatus",
                table: "Accounts");

            migrationBuilder.AddColumn<int>(
                name: "AccountStatus",
                table: "Customers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountStatus",
                table: "Customers");

            migrationBuilder.AddColumn<int>(
                name: "AccountStatus",
                table: "Accounts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
