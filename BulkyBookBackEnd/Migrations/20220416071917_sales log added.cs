using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BulkyBookBackEnd.Migrations
{
    public partial class saleslogadded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SalesLogId",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SalesLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Day = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesLog", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_SalesLogId",
                table: "Orders",
                column: "SalesLogId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesLog_Day_Month_Year",
                table: "SalesLog",
                columns: new[] { "Day", "Month", "Year" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_SalesLog_SalesLogId",
                table: "Orders",
                column: "SalesLogId",
                principalTable: "SalesLog",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_SalesLog_SalesLogId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "SalesLog");

            migrationBuilder.DropIndex(
                name: "IX_Orders_SalesLogId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SalesLogId",
                table: "Orders");
        }
    }
}
