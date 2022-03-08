using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopping2022.Migrations
{
    public partial class addCategorySurnameAndAddres : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Surnames",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "Surnames",
                table: "Categories");
        }
    }
}
