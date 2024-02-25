using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatBot.Migrations
{
    public partial class AccountCurrency : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Accounts");
        }
    }
}
