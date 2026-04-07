namespace ErnyosKozoApi.Models
{
    // ===== KOMMENT ENTITÁS =====
    // Az adatbázis "Kommentek" táblájának C# leképezése
    // Ez a projekt legösszetettebb modellje – önhivatkozó (self-referencing) kapcsolatot tartalmaz
    // a fa struktúrájú komment hierarchiához (válasz a válaszra)
    public class Komment
    {
        // ===== ELSŐDLEGES KULCS =====
        public int KommentID { get; set; }

        // ===== IDEGEN KULCS – BEJEGYZÉS (KÖTELEZŐ) =====
        // int (nem nullable) → minden komment KÖTELEZŐEN tartozik egy bejegyzéshez
        public int BejegyzesID { get; set; }

        // Navigációs property – a kapcsolódó bejegyzés
        // Az AppDbContext-ben Cascade törlésre van konfigurálva:
        // bejegyzés törlésekor az összes kommentje is törlődik
        public Bejegyzes? Bejegyzes { get; set; }

        // ===== IDEGEN KULCS – FELHASZNÁLÓ / SZERZŐ (KÖTELEZŐ) =====
        // int (nem nullable) → minden kommentnek KÖTELEZŐEN van szerzője
        public int FelhasznaloID { get; set; }
        // Navigációs property – a komment szerzője
        // Az AppDbContext-ben Restrict törlésre van konfigurálva:
        // felhasználó nem törölhető amíg vannak kommentjei (az AdminController manuálisan kezeli)
        public Felhasznalo? Felhasznalo { get; set; }

        // ===== TARTALMI MEZŐ =====
        public string? Tartalom { get; set; } // A komment szövege

        // ===== LÉTREHOZÁS DÁTUMA =====
        public DateTime Letrehozva { get; set; } = DateTime.UtcNow; // Alapértelmezett: aktuális UTC idő


        // ===== ÖNHIVATKOZÓ KAPCSOLAT (SELF-REFERENCING RELATIONSHIP) =====
        // Ez teszi lehetővé a kommentek fa struktúráját:
        // Komment A (gyökér)
        //   └── Komment B (válasz A-ra)
        //       └── Komment C (válasz B-re)
        //   └── Komment D (másik válasz A-ra)

        // int? (nullable FK) → ha null, akkor ez egy gyökér szintű komment (nem válasz semmire)
        //                     → ha kitöltve, akkor ez egy válasz a megadott ID-jú kommentre
        public int? SzuloKommentID { get; set; }

        // Navigációs property FELFELÉ → melyik kommentre válaszol ez a komment
        // Az AppDbContext-ben Restrict törlésre van konfigurálva
        public Komment? SzuloKomment { get; set; }

        // Navigációs property LEFELÉ → az erre a kommentre érkezett válaszok gyűjteménye
        // ICollection<Komment>: ugyanolyan típus, mint az osztály maga → önhivatkozás
        // A ForumController.GetKommentek() rekurzív BuildTree metódusa használja ezt a struktúrát
        // "= new List<Komment>()" → inicializálás, hogy üres lista legyen null helyett
        public ICollection<Komment> Valaszok { get; set; } = new List<Komment>();
    }
}