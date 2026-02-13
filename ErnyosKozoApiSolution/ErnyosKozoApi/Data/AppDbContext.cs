using ErnyosKozoApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace ErnyosKozoApi.Data
{

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Felhasznalo> Felhasznalok { get; set; }
        public DbSet<Spot> Spotok { get; set; }
        public DbSet<Utvonal> Utvonalak { get; set; }
    }

}
