namespace ErnyosKozoApi.Models
{
    // ===== FELHASZNÁLÓ ENTITÁS =====
    // Az adatbázis "Felhasznalok" táblájának C# leképezése
    // Ez a legfontosabb modell – szinte minden más entitás hivatkozik rá
    public class Felhasznalo
    {
        // ===== ELSŐDLEGES KULCS (PRIMARY KEY) =====
        // EF Core konvenció: "{Osztálynév}ID" → automatikusan PK + IDENTITY (auto-increment)
        public int FelhasznaloID { get; set; }

        // ===== BEJELENTKEZÉSI ÉS AZONOSÍTÁSI ADATOK =====
        // string? (nullable) → az adatbázisban NULL-ként tárolható
        public string? FelhasznaloNev { get; set; } // Egyedi felhasználónév (a bejelentkezéshez)
        public string? TeljesNev { get; set; }      // Megjelenítendő név (pl. "Kiss Péter")
        public string? Email { get; set; }          // E-mail cím

        // DateTime? (nullable DateTime) → opcionális születési dátum
        public DateTime? SzuletesiDatum { get; set; }

        // ===== JELSZÓ HASH =====
        // SOHA nem a nyers jelszót tároljuk, hanem annak hash-elt változatát!
        // A PasswordHasher<Felhasznalo> az AuthController-ben végzi a hashelést
        // string (nem nullable!) → minden felhasználónak KÖTELEZŐEN van jelszó hash-e
        // "= string.Empty" → alapértelmezett üres string, hogy ne legyen null regisztrációig
        public string PasswordHash { get; set; } = string.Empty;

        // ===== REGISZTRÁCIÓ DÁTUMA =====
        // DateTime? (nullable) alapértelmezett értékkel
        // Az AuthController.Register() metódus is beállítja DateTime.UtcNow-ra
        public DateTime? RegDatum { get; set; } = DateTime.UtcNow;

        // ===== PROFIL ADATOK =====
        // Mind opcionális mezők (string? → nullable) – a felhasználó később töltheti ki
        public string? Bio { get; set; }        // Rövid bemutatkozás szöveg
        public string? Helyszin { get; set; }   // Lakóhely / tartózkodási hely
        public string? Klub { get; set; }       // Melyik repülős klubhoz tartozik

        // ===== PROFILKÉP ÉS BORÍTÓKÉP URL-EK =====
        // A feltöltött képek URL-jeit tároljuk (a képfájlok a wwwroot/uploads mappában vannak)
        // null → alapértelmezett kép jelenik meg a frontenden
        public string? AvatarUrl { get; set; }  // Profilkép URL-je
        public string? CoverUrl { get; set; }   // Borítókép URL-je

        // ===== ADMIN JOGOSULTSÁG =====
        // bool típus → az adatbázisban BIT oszlop (0 vagy 1)
        // "= false" → alapértelmezetten minden új felhasználó NEM admin
        // Az AuthController.Register() is explicit false-ra állítja (kettős biztonság)
        // Az AdminController-ben módosítható (admin kinevezés/visszavonás)
        public bool IsAdmin { get; set; } = false;
    }
}