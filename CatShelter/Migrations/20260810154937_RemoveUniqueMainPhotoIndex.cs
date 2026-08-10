using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatShelter.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUniqueMainPhotoIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Photos_AnimalId",
                table: "Photos");

            migrationBuilder.CreateIndex(
                name: "IX_Photos_AnimalId",
                table: "Photos",
                column: "AnimalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Photos_AnimalId",
                table: "Photos");

            migrationBuilder.CreateIndex(
                name: "IX_Photos_AnimalId",
                table: "Photos",
                column: "AnimalId",
                unique: true,
                filter: " \"IsMain\" = true ");
        }
    }
}
