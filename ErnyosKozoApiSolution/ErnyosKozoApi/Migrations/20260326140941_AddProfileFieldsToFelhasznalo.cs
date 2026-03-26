using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErnyosKozoApi.Migrations
{
    public partial class AddProfileFieldsToFelhasznalo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "Felhasznalok",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "Felhasznalok",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoverUrl",
                table: "Felhasznalok",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Helyszin",
                table: "Felhasznalok",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Klub",
                table: "Felhasznalok",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "Felhasznalok");

            migrationBuilder.DropColumn(
                name: "Bio",
                table: "Felhasznalok");

            migrationBuilder.DropColumn(
                name: "CoverUrl",
                table: "Felhasznalok");

            migrationBuilder.DropColumn(
                name: "Helyszin",
                table: "Felhasznalok");

            migrationBuilder.DropColumn(
                name: "Klub",
                table: "Felhasznalok");
        }
    }
}