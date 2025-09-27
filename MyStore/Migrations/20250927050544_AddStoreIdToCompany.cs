using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyStore.Migrations
{
    /// <inheritdoc />
    public partial class AddStoreIdToCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Companies");

            migrationBuilder.AddColumn<int>(
                name: "StoreId",
                table: "Companies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Companies_StoreId",
                table: "Companies",
                column: "StoreId");

            migrationBuilder.AddForeignKey(
                name: "FK_Companies_Stores_StoreId",
                table: "Companies",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Companies_Stores_StoreId",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_Companies_StoreId",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "Companies");

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
