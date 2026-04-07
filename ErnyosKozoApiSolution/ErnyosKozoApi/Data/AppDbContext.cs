using ErnyosKozoApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ErnyosKozoApi.Data
{
    // ===== AZ ADATBÁZIS KONTEXTUS =====
    // Az AppDbContext az alkalmazás és az adatbázis közötti "híd"
    // A DbContext az EF Core (Entity Framework Core) központi osztálya, amely:
    // - Kezeli az adatbázis-kapcsolatot
    // - Leképezi a C# osztályokat adatbázis táblákra
    // - Nyomon követi az entitások változásait (Change Tracking)
    // - SQL lekérdezéseket generál a LINQ kifejezésekből
    public class AppDbContext : DbContext
    {
        // Konstruktor – a DbContextOptions tartalmazza a kapcsolódási beállításokat
        // (pl. connection string, adatbázis provider – ezeket a Program.cs-ben konfiguráltuk)
        // A ": base(options)" továbbítja a beállításokat az ős DbContext osztálynak
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        // ===== DBSET-EK (TÁBLÁK) =====
        // Minden DbSet<T> egy adatbázis táblát reprezentál
        // A property neve lesz az alapértelmezett táblanév az adatbázisban
        // Ezeken keresztül végezzük a CRUD műveleteket a controllerekben
        public DbSet<Felhasznalo> Felhasznalok { get; set; }    // Felhasználók tábla
        public DbSet<Spot> Spotok { get; set; }                 // Repülős helyszínek tábla
        public DbSet<SpotJavaslat> SpotJavaslatok { get; set; } // Felhasználói spot javaslatok tábla
        public DbSet<Utvonal> Utvonalak { get; set; }           // Útvonalak tábla
        public DbSet<Hir> Hirek { get; set; }                   // Hírek tábla
        public DbSet<Bejegyzes> Bejegyzesek { get; set; }       // Fórum bejegyzések tábla
        public DbSet<Komment> Kommentek { get; set; }           // Kommentek tábla
        public DbSet<Kovetes> Kovetesek { get; set; }           // Követések tábla (many-to-many kapcsolat)

        // ===== FLUENT API KONFIGURÁCIÓ =====
        // Az OnModelCreating metódusban a Fluent API segítségével finomhangoljuk
        // az adatbázis sémát – olyan dolgokat, amelyeket Data Annotation-ökkel (attribútumokkal)
        // nem vagy nehezen lehetne megoldani
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Az ős osztály OnModelCreating metódusának meghívása (ajánlott gyakorlat)
            base.OnModelCreating(modelBuilder);

            // ===== KOMMENT – SZÜLŐ KOMMENT KAPCSOLAT (önhivatkozó / self-referencing) =====
            // Egy kommentnek lehet egy szülő kommentje (amelyre válaszol)
            // Ez teszi lehetővé a fa (tree) struktúrájú komment hierarchiát
            modelBuilder.Entity<Komment>()
                .HasOne(k => k.SzuloKomment)            // Egy kommentnek van egy szülője...
                .WithMany(k => k.Valaszok)              // ...és egy szülőnek sok válasza lehet
                .HasForeignKey(k => k.SzuloKommentID)   // Az idegen kulcs (foreign key) mező
                .OnDelete(DeleteBehavior.Restrict);     // Törlési viselkedés: RESTRICT
                // Restrict = nem engedi törölni a szülő kommentet, amíg vannak válaszai
                // (Cascade itt körkörös törlést okozna → SQL Server nem engedné)
            
            // ===== KOMMENT – FELHASZNÁLÓ KAPCSOLAT =====
            // Egy kommentet egy felhasználó ír
            modelBuilder.Entity<Komment>()
                .HasOne(k => k.Felhasznalo)             // Egy kommentnek van egy szerzője...
                .WithMany()                             // ...de a Felhasznalo osztályban nincs visszahivatkozó lista
                .HasForeignKey(k => k.FelhasznaloID)
                .OnDelete(DeleteBehavior.Restrict);     // Restrict: felhasználó törlésénél nem törlődnek automatikusan a kommentek
            // Ez azért kell, mert a Bejegyzes→Komment már Cascade, és SQL Server nem engedi
            // hogy két Cascade útvonal vezessen ugyanahhoz a táblához (multiple cascade paths)

            // ===== KOMMENT – BEJEGYZÉS KAPCSOLAT =====
            // Egy bejegyzéshez sok komment tartozhat
            modelBuilder.Entity<Komment>()
                .HasOne(k => k.Bejegyzes)               // Egy komment egy bejegyzéshez tartozik...
                .WithMany(b => b.Kommentek)             // ...és egy bejegyzésnek sok kommentje van
                .HasForeignKey(k => k.BejegyzesID)
                .OnDelete(DeleteBehavior.Cascade);      // Cascade: bejegyzés törlésekor az ÖSSZES kommentje is törlődik
                                                        // Ez logikus: ha kitöröljük a bejegyzést, a kommentek is értelmetlenné válnak

            // ===== KÖVETÉS – ÖSSZETETT EGYEDI INDEX =====
            // Biztosítja, hogy egy felhasználó csak egyszer követhessen egy másik felhasználót
            // Adatbázis szintű védelem – még ha a kód hibás lenne, az adatbázis nem engedi a duplikációt
            modelBuilder.Entity<Kovetes>()
                .HasIndex(k => new { k.KovetoFelhasznaloID, k.KovetettFelhasznaloID }) // Összetett index (composite index)
                .IsUnique();    // UNIQUE constraint: a két mező kombinációja egyedi kell legyen
                // Pl.: (1, 2) létezhet, (2, 1) is létezhet, de (1, 2) másodszor NEM
        }
    }
}