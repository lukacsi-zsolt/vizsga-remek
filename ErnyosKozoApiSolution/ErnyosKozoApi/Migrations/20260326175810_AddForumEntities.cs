using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErnyosKozoApi.Migrations
{
    /// <inheritdoc />
    public partial class AddForumEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Lat",
                table: "Spotok",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LetrehozoFelhasznaloID",
                table: "Spotok",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Lon",
                table: "Spotok",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Spotok",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Bejegyzesek",
                columns: table => new
                {
                    BejegyzesID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FelhasznaloID = table.Column<int>(type: "int", nullable: false),
                    Cim = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tartalom = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KepUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Letrehozva = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SpotID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bejegyzesek", x => x.BejegyzesID);
                    table.ForeignKey(
                        name: "FK_Bejegyzesek_Felhasznalok_FelhasznaloID",
                        column: x => x.FelhasznaloID,
                        principalTable: "Felhasznalok",
                        principalColumn: "FelhasznaloID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Bejegyzesek_Spotok_SpotID",
                        column: x => x.SpotID,
                        principalTable: "Spotok",
                        principalColumn: "SpotID");
                });

            migrationBuilder.CreateTable(
                name: "Kovetesek",
                columns: table => new
                {
                    KovetesID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KovetoFelhasznaloID = table.Column<int>(type: "int", nullable: false),
                    KovetettFelhasznaloID = table.Column<int>(type: "int", nullable: false),
                    Letrehozva = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kovetesek", x => x.KovetesID);
                });

            migrationBuilder.CreateTable(
                name: "Kommentek",
                columns: table => new
                {
                    KommentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BejegyzesID = table.Column<int>(type: "int", nullable: false),
                    FelhasznaloID = table.Column<int>(type: "int", nullable: false),
                    Tartalom = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Letrehozva = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SzuloKommentID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kommentek", x => x.KommentID);
                    table.ForeignKey(
                        name: "FK_Kommentek_Bejegyzesek_BejegyzesID",
                        column: x => x.BejegyzesID,
                        principalTable: "Bejegyzesek",
                        principalColumn: "BejegyzesID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Kommentek_Felhasznalok_FelhasznaloID",
                        column: x => x.FelhasznaloID,
                        principalTable: "Felhasznalok",
                        principalColumn: "FelhasznaloID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Kommentek_Kommentek_SzuloKommentID",
                        column: x => x.SzuloKommentID,
                        principalTable: "Kommentek",
                        principalColumn: "KommentID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bejegyzesek_FelhasznaloID",
                table: "Bejegyzesek",
                column: "FelhasznaloID");

            migrationBuilder.CreateIndex(
                name: "IX_Bejegyzesek_SpotID",
                table: "Bejegyzesek",
                column: "SpotID");

            migrationBuilder.CreateIndex(
                name: "IX_Kommentek_BejegyzesID",
                table: "Kommentek",
                column: "BejegyzesID");

            migrationBuilder.CreateIndex(
                name: "IX_Kommentek_FelhasznaloID",
                table: "Kommentek",
                column: "FelhasznaloID");

            migrationBuilder.CreateIndex(
                name: "IX_Kommentek_SzuloKommentID",
                table: "Kommentek",
                column: "SzuloKommentID");

            migrationBuilder.CreateIndex(
                name: "IX_Kovetesek_KovetoFelhasznaloID_KovetettFelhasznaloID",
                table: "Kovetesek",
                columns: new[] { "KovetoFelhasznaloID", "KovetettFelhasznaloID" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Kommentek");

            migrationBuilder.DropTable(
                name: "Kovetesek");

            migrationBuilder.DropTable(
                name: "Bejegyzesek");

            migrationBuilder.DropColumn(
                name: "Lat",
                table: "Spotok");

            migrationBuilder.DropColumn(
                name: "LetrehozoFelhasznaloID",
                table: "Spotok");

            migrationBuilder.DropColumn(
                name: "Lon",
                table: "Spotok");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Spotok");
        }
    }
}
