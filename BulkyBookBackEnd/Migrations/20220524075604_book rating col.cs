using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BulkyBookBackEnd.Migrations
{
    public partial class bookratingcol : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NoOfRaters",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "RatePoints",
                table: "Books");

            migrationBuilder.RenameColumn(
                name: "Rating",
                table: "Books",
                newName: "FinalRating");

            migrationBuilder.CreateTable(
                name: "BookRatings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookRatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookRatings_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookRatings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookRatings_BookId",
                table: "BookRatings",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_BookRatings_UserId_BookId",
                table: "BookRatings",
                columns: new[] { "UserId", "BookId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookRatings");

            migrationBuilder.RenameColumn(
                name: "FinalRating",
                table: "Books",
                newName: "Rating");

            migrationBuilder.AddColumn<int>(
                name: "NoOfRaters",
                table: "Books",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "RatePoints",
                table: "Books",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
