namespace ErnyosKozoApi.Models
{
    public class Komment
    {
        public int KommentID { get; set; }

        public int BejegyzesID { get; set; }
        public Bejegyzes? Bejegyzes { get; set; }

        public int FelhasznaloID { get; set; }
        public Felhasznalo? Felhasznalo { get; set; }

        public string? Tartalom { get; set; }
        public DateTime Letrehozva { get; set; } = DateTime.UtcNow;

        public int? SzuloKommentID { get; set; }
        public Komment? SzuloKomment { get; set; }

        public ICollection<Komment> Valaszok { get; set; } = new List<Komment>();
    }
}