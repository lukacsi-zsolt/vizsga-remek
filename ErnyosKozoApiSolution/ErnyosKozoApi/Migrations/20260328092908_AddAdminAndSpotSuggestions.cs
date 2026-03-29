using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErnyosKozoApi.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminAndSpotSuggestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "Felhasznalok",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "SpotJavaslatok",
                columns: table => new
                {
                    SpotJavaslatID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nev = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Orszag = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Megye = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HelyLeiras = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Magassag = table.Column<int>(type: "int", nullable: true),
                    AtlagSzel = table.Column<double>(type: "float", nullable: true),
                    Szabalyok = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Lat = table.Column<double>(type: "float", nullable: true),
                    Lon = table.Column<double>(type: "float", nullable: true),
                    BekuldoFelhasznaloID = table.Column<int>(type: "int", nullable: true),
                    Letrehozva = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Feldolgozva = table.Column<bool>(type: "bit", nullable: false),
                    AdminMegjegyzes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpotJavaslatok", x => x.SpotJavaslatID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SpotJavaslatok");

            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "Felhasznalok");
        }
    }
}
