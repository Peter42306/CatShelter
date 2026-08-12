using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatShelter.Migrations
{
    /// <inheritdoc />
    public partial class AddBlogPublishedAtUtc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedAtUtc",
                table: "BlogPosts",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublishedAtUtc",
                table: "BlogPosts");
        }
    }
}
