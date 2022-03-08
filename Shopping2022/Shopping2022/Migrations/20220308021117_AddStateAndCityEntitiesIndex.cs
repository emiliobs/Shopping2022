using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopping2022.Migrations
{
    public partial class AddStateAndCityEntitiesIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_States_Name_Id",
                table: "States");

            migrationBuilder.DropIndex(
                name: "IX_Cities_Name_Id",
                table: "Cities");

            migrationBuilder.CreateIndex(
                name: "IX_States_Name_CountryId",
                table: "States",
                columns: new[] { "Name", "CountryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cities_Name_StateId",
                table: "Cities",
                columns: new[] { "Name", "StateId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_States_Name_CountryId",
                table: "States");

            migrationBuilder.DropIndex(
                name: "IX_Cities_Name_StateId",
                table: "Cities");

            migrationBuilder.CreateIndex(
                name: "IX_States_Name_Id",
                table: "States",
                columns: new[] { "Name", "Id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cities_Name_Id",
                table: "Cities",
                columns: new[] { "Name", "Id" },
                unique: true);
        }
    }
}
