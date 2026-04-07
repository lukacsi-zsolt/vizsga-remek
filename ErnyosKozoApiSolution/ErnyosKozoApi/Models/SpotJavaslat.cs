namespace ErnyosKozoApi.Models
{
    // ===== SPOT JAVASLAT ENTITÁS =====
    // Az adatbázis "SpotJavaslatok" táblájának C# leképezése
    // Amikor egy felhasználó új helyszínt javasol, az először IDE kerül (nem közvetlenül a Spotok táblába)
    // Az admin később elfogadja (→ Spot-tá alakítja) vagy elutasítja (→ törli)
    // Ez egy "moderációs minta" (moderation pattern) – a tartalom jóváhagyás után válik publikussá
    public class SpotJavaslat
    {
        // ===== ELSŐDLEGES KULCS =====
        public int SpotJavaslatID { get; set; }

        // ===== HELYSZÍN ADATAI =====
        // Ugyanazok a mezők, mint a Spot entitásban – az admin elfogadásakor átmásolódnak
        // (lásd AdminController.ApproveSpotSuggestion())
        public string? Nev { get; set; }        // Javasolt helyszín neve
        public string? Orszag { get; set; }     // Ország
        public string? Megye { get; set; }      // Megye/régió
        public string? HelyLeiras { get; set; } // Megközelítés leírása
        public int? Magassag { get; set; }      // Magasság méterben
        public double? AtlagSzel { get; set; }  // Átlagos szélsebesség
        public string? Szabalyok { get; set; }  // Helyi szabályok

        // ===== GPS KOORDINÁTÁK =====
        public double? Lat { get; set; }        // Földrajzi szélesség
        public double? Lon { get; set; }        // Földrajzi hosszúság

        // ===== BEKÜLDŐ FELHASZNÁLÓ =====
        // int? (nullable FK) → ki javasolta a helyszínt
        // A SpotokController.SuggestSpot() mindig beállítja a tokenből,
        // de nullable, mert ha a felhasználó törlődik, a javaslat megmaradhat
        public int? BekuldoFelhasznaloID { get; set; }

        // ===== BEKÜLDÉS IDŐPONTJA =====
        public DateTime Letrehozva { get; set; } = DateTime.UtcNow;

        // ===== FELDOLGOZÁSI ÁLLAPOT =====
        // false → még nem döntött róla az admin (megjelenik a "feldolgozandó" listában)
        // true → az admin már elfogadta (AdminController.ApproveSpotSuggestion() állítja true-ra)
        // Az AdminController.GetDashboard() a feldolgozatlanok számát mutatja: CountAsync(x => !x.Feldolgozva)
        public bool Feldolgozva { get; set; } = false;

        // ===== ADMIN MEGJEGYZÉS =====
        // Az admin opcionálisan fűzhet megjegyzést a javaslathoz (pl. elutasítás indoklása)
        public string? AdminMegjegyzes { get; set; }
    }
}