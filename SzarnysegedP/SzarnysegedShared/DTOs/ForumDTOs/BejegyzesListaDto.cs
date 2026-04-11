// ===== BEJEGYZÉS LISTA DTO =====
// A fórum bejegyzések listázásához használt DTO – az összes szükséges adat egy szinten
// A ForumController.GetBejegyzesek() és GetBejegyzes() állítja össze
// A Forum.razor és ForumBejegyzes.razor jeleníti meg
//
// Ez egy "laposított" (flattened) DTO:
// Ahelyett, hogy beágyazott objektumokat használnánk (Bejegyzes.Felhasznalo.FelhasznaloNev),
// az összes szükséges mezőt egy szintre hozzuk (FelhasznaloNev, TeljesNev, AvatarUrl)
// Ez egyszerűsíti a JSON szerializálást és a kliens oldali adatkezelést
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SzarnysegedShared.DTOs.ForumDTOs
{
    public class BejegyzesListaDto
    {
        public int BejegyzesID { get; set; }        // A bejegyzés saját adatai
        public string? Cim { get; set; }            // A bejegyzés címe
        public string? Tartalom { get; set; }       // A bejegyzés szöveges tartalma
        public string? KepUrl { get; set; }         // Csatolt kép URL-je (opcionális)
        public DateTime Letrehozva { get; set; }    // Létrehozás időpontja

        public int FelhasznaloID { get; set; }      // A SZERZŐ adatai (a Felhasznalo entitásból "laposítva")
                                                    // Az Include(b => b.Felhasznalo) betölti, majd a Select() kimásolja ide
        public string? FelhasznaloNev { get; set; } // Szerző felhasználóneve
        public string? TeljesNev { get; set; }      // Szerző teljes neve
        public string? AvatarUrl { get; set; }      // Szerző profilképe

        public int? SpotID { get; set; }            // A SPOT adatai (a Spot entitásból "laposítva")
                                                    // Nullable, mert nem minden bejegyzés tartozik spothoz
        public string? SpotNev { get; set; }        // Spot neve (pl. "Dobogókő")
        public string? SpotSlug { get; set; }       // Spot slug-ja (pl. "dobogoko") – linkekhez

        // Összesített adat – az Include(b => b.Kommentek) + .Count-ból számolva
        // Nem a kommentek tartalmát küldjük, csak a SZÁMUKAT (hatékonyabb)
        public int KommentekSzama { get; set; }
    }
}
