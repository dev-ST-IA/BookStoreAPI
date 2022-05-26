using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BulkyBookBackEnd.Migrations
{
    public partial class feedbacksuser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Feedbacks",
                table: "Users");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Feedbacks",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
