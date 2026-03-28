using ErnyosKozoApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ErnyosKozoApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Felhasznalo> Felhasznalok { get; set; }
        public DbSet<Spot> Spotok { get; set; }
        public DbSet<SpotJavaslat> SpotJavaslatok { get; set; }
        public DbSet<Utvonal> Utvonalak { get; set; }
        public DbSet<Hir> Hirek { get; set; }
        public DbSet<Bejegyzes> Bejegyzesek { get; set; }
        public DbSet<Komment> Kommentek { get; set; }
        public DbSet<Kovetes> Kovetesek { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Komment>()
                .HasOne(k => k.SzuloKomment)
                .WithMany(k => k.Valaszok)
                .HasForeignKey(k => k.SzuloKommentID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Komment>()
                .HasOne(k => k.Felhasznalo)
                .WithMany()
                .HasForeignKey(k => k.FelhasznaloID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Komment>()
                .HasOne(k => k.Bejegyzes)
                .WithMany(b => b.Kommentek)
                .HasForeignKey(k => k.BejegyzesID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Kovetes>()
                .HasIndex(k => new { k.KovetoFelhasznaloID, k.KovetettFelhasznaloID })
                .IsUnique();
        }
    }
}