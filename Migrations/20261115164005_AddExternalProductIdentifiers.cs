using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FTELMarket.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalProductIdentifiers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ExternalId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reference",
                table: "Products",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_ExternalId",
                table: "Products",
                column: "ExternalId",
                unique: true,
                filter: "[ExternalId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_ExternalId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Reference",
                table: "Products");
        }
    }
}
