// FÓRUM DTO-K
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SzarnysegedShared.DTOs.ForumDTOs
{
    // ===== KOMMENT DTO (FA STRUKTÚRA) =====
    // A kommentek hierarchikus (fa) megjelenítéséhez használt DTO
    // A ForumController.GetKommentek() rekurzív BuildTree() metódusa állítja össze
    // A ForumBejegyzes.razor RenderKomment() metódusa jeleníti meg rekurzívan
    public class KommentDto
    {
        public int KommentID { get; set; }
        public int BejegyzesID { get; set; }        // Melyik bejegyzéshez tartozik
        public int? SzuloKommentID { get; set; }    // Szülő komment ID – null ha gyökér szintű komment

        // ===== "LAPOSÍTOTT" FELHASZNÁLÓ ADATOK =====
        // A szerző adatait közvetlenül a DTO-ba másoljuk (nem beágyazott FelhasznaloDTO-ként)
        // Ez egyszerűsíti a kliens oldali kezelést (nem kell komment.Felhasznalo.FelhasznaloNev)

        public int FelhasznaloID { get; set; }
        public string? FelhasznaloNev { get; set; } // A komment szerzőjének felhasználóneve
        public string? TeljesNev { get; set; }      // A komment szerzőjének teljes neve
        public string? AvatarUrl { get; set; }      // A komment szerzőjének profilképe
        public string? Tartalom { get; set; }       // A komment szövege
        public DateTime Letrehozva { get; set; }    // A komment írásának időpontja

        // ===== REKURZÍV PROPERTY =====
        // A válaszok listája – UGYANOLYAN típusú, mint az osztály maga!
        // Ez teszi lehetővé a fa struktúra JSON-ként való szerializálását
        // Az API rekurzívan felépíti (BuildTree), a kliens rekurzívan rendereli (RenderKomment)
        // "= new()" → üres lista alapértelmezetten (ha nincsenek válaszok)
        public List<KommentDto> Valaszok { get; set; } = new();
    }
}
