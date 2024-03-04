using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatBot.Migrations
{
    public partial class AddComplaintStatusToComplaintModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ComplaintStatus",
                table: "Complaints",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ComplaintStatus",
                table: "Complaints");
        }
    }
}
