using System.Xml.Linq;

namespace ErnyosKozoApi.Models
{
    // ===== BEJEGYZÉS ENTITÁS (FÓRUM POSZT) =====
    // Ez az osztály az adatbázis "Bejegyzesek" táblájának C# leképezése
    // Az EF Core az osztály property-jeit oszlopokká alakítja az adatbázisban
    public class Bejegyzes
    {
        // ===== ELSŐDLEGES KULCS (PRIMARY KEY) =====
        // Az EF Core konvenció szerint a "{Osztálynév}ID" nevű int property automatikusan PK lesz
        // Az adatbázis IDENTITY-ként kezeli → automatikusan növekvő szám (1, 2, 3...)
        public int BejegyzesID { get; set; }

        // ===== IDEGEN KULCS (FOREIGN KEY) – FELHASZNÁLÓ =====
        // Melyik felhasználó írta a bejegyzést
        // int (nem nullable) → minden bejegyzésnek KÖTELEZŐEN van szerzője
        public int FelhasznaloID { get; set; }

        // ===== NAVIGÁCIÓS PROPERTY – FELHASZNÁLÓ =====
        // Az EF Core ezen keresztül tölti be a kapcsolódó Felhasznalo entitást (Include/JOIN)
        // Felhasznalo? (nullable) → nem mindig van betöltve (csak ha Include-ot használunk)
        // Ez NEM oszlop az adatbázisban, hanem az EF Core navigációs mechanizmusa
        public Felhasznalo? Felhasznalo { get; set; }

        // ===== BEJEGYZÉS TARTALMI MEZŐI =====
        // string? (nullable) → opcionális mezők, lehetnek null értékűek az adatbázisban
        public string? Cim { get; set; }    // A bejegyzés címe
        public string? Tartalom { get; set; }   // A bejegyzés szöveges tartalma
        public string? KepUrl { get; set; }     // Csatolt kép URL-je (opcionális)

        // ===== LÉTREHOZÁS DÁTUMA =====
        // Alapértelmezett érték: DateTime.UtcNow → ha nem állítjuk be, az aktuális UTC időt kapja
        // A controller is felülírja ezt szerver oldalon (DateTime.UtcNow), szóval kettős biztonság
        public DateTime Letrehozva { get; set; } = DateTime.UtcNow;

        // ===== IDEGEN KULCS – SPOT (OPCIONÁLIS) =====
        // int? (nullable FK) → a bejegyzés OPCIONÁLISAN tartozhat egy spothoz
        // Ha null → általános fórum bejegyzés, ha kitöltve → egy adott helyszínhez kapcsolódó poszt
        public int? SpotID { get; set; }

        // Navigációs property a kapcsolódó Spot-hoz
        public Spot? Spot { get; set; }

        // ===== NAVIGÁCIÓS PROPERTY – KOMMENTEK (EGY-A-TÖBBHÖZ / ONE-TO-MANY) =====
        // Egy bejegyzéshez sok komment tartozhat
        // ICollection<Komment>: gyűjtemény típus, amelyet az EF Core automatikusan feltölt Include-nál
        // "= new List<Komment>()" → inicializálás, hogy ne legyen NullReferenceException
        // ha a kommentek nem lettek betöltve (Include nélkül üres lista lesz, nem null)
        public ICollection<Komment> Kommentek { get; set; } = new List<Komment>();
    }
}