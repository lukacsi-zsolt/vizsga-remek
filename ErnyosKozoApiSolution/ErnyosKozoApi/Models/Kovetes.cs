namespace ErnyosKozoApi.Models
{
    // ===== KÖVETÉS ENTITÁS (KAPCSOLÓTÁBLA) =====
    // Az adatbázis "Kovetesek" táblájának C# leképezése
    // Ez egy kapcsolótábla (junction/join table) a felhasználók közötti many-to-many kapcsolathoz
    // Egy rekord azt jelenti: "X felhasználó követi Y felhasználót"
    public class Kovetes
    {
        // ===== ELSŐDLEGES KULCS =====
        public int KovetesID { get; set; }

        // ===== KI KÖVET (KÖVETŐ) =====
        // Az a felhasználó, aki a "Követés" gombra kattintott
        // int (nem nullable) → kötelező mező
        public int KovetoFelhasznaloID { get; set; }
        // ===== KIT KÖVET (KÖVETETT) =====
        // Az a felhasználó, akit követnek
        public int KovetettFelhasznaloID { get; set; }

        // ===== KÖVETÉS IDŐPONTJA =====
        public DateTime Letrehozva { get; set; } = DateTime.UtcNow;

        // ===== MEGJEGYZÉSEK =====
        // 1. Nincsenek navigációs property-k (Felhasznalo? Koveto, Felhasznalo? Kovetett)
        //    → a FelhasznalokController-ben ezért kell explicit LINQ Join-t használni
        //    Ha lennének navigációs property-k, egyszerűbb Include()-dal is meg lehetne oldani
        //
        // 2. Az AppDbContext-ben van egy COMPOSITE UNIQUE INDEX erre a két mezőre:
        //    .HasIndex(k => new { k.KovetoFelhasznaloID, k.KovetettFelhasznaloID }).IsUnique()
        //    Ez adatbázis szinten garantálja, hogy egy felhasználó csak egyszer követhet valakit
        //
        // 3. A KovetesekController.KovetesValtas() toggle logikát használ:
        //    ha létezik a rekord → törlés (kikövetés), ha nem létezik → létrehozás (bekövetés)
    }
}