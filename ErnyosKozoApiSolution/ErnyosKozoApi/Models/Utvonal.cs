namespace ErnyosKozoApi.Models
{
    // ===== ÚTVONAL ENTITÁS =====
    // Az adatbázis "Utvonalak" táblájának C# leképezése
    // Egy felhasználó utazását rögzíti egy adott spothoz (repülős helyszínhez)
    public class Utvonal
    {
        // ===== ELSŐDLEGES KULCS =====
        public int UtvonalID { get; set; }

        // ===== IDEGEN KULCSOK (KÖTELEZŐ) =====
        // int (nem nullable) → minden útvonalnak KÖTELEZŐEN van felhasználója és spotja
        public int FelhasznaloID { get; set; }  // Ki utazott (melyik felhasználó)
        public int SpotID { get; set; }         // Hová utazott (melyik repülős helyszínre)

        // MEGJEGYZÉS: nincsenek navigációs property-k (Felhasznalo?, Spot?)
        // Ez azt jelenti, hogy Include()-dal nem lehet betölteni a kapcsolódó entitásokat
        // Ha szükség lenne rájuk, explicit LINQ Join-t kellene használni
        // (vagy hozzá kellene adni a navigációs property-ket)

        // ===== UTAZÁSI ADATOK =====
        // DateTime? (nullable) → opcionális időpontok
        public DateTime? IndulasIdo { get; set; }   // Mikor indult el
        public DateTime? ErkezesIdo { get; set; }   // Mikor érkezett meg

        // double? (nullable) → opcionális távolság
        public double? TavolsagKM { get; set; } // Megtett távolság kilométerben

        // ===== MEGJEGYZÉS =====
        // string (nem nullable!) → az adatbázisban NOT NULL oszlop
        // FIGYELEM: nincs "= string.Empty" alapértelmezett érték és nincs "?" nullable jelölés
        // Ez azt jelenti, hogy az EF Core NOT NULL constraint-et hoz létre
        // Ha a kliens nem küld értéket, SQL hiba keletkezhet (érdemes lenne string?-re cserélni)
        public string Megjegyzes { get; set; }
    }

}
