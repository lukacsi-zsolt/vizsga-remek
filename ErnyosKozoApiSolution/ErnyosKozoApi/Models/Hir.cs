namespace ErnyosKozoApi.Models
{
    // ===== HÍR ENTITÁS =====
    // Az adatbázis "Hirek" táblájának C# leképezése
    // Egyszerű, önálló entitás – nincs kapcsolata más táblákkal (nincsenek FK-k és navigációs property-k)
    public class Hir
    {
        // ===== ELSŐDLEGES KULCS =====
        // EF Core konvenció: "{Osztálynév}ID" → automatikus PK + IDENTITY
        public int HirID { get; set; }

        // ===== TARTALMI MEZŐK =====
        // string? (nullable) → opcionális szöveges mezők
        public string? Cim { get; set; }         // A hír címe
        public string? Tartalom { get; set; }    // A hír szöveges tartalma (lehet HTML is a frontenden)
        public string? KepUrl {get; set; }       // Kiemelt kép URL-je (opcionális)
        public string? Kategoria {get; set; }    // Hír kategóriája (pl. "verseny", "közlemény", "időjárás")

        // ===== MEGJELENÉS DÁTUMA =====
        // DateTime (nem nullable!) → minden hírnek KÖTELEZŐEN van dátuma
        // A HirekController.Create()-ben ha a kliens nem küld dátumot (default),
        // akkor DateTime.UtcNow-t kap
        public DateTime Datum {get; set;}
    }

}
