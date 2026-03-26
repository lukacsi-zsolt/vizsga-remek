using System.Xml.Linq;

namespace ErnyosKozoApi.Models
{
    public class Bejegyzes
    {
        public int BejegyzesID { get; set; }

        public int FelhasznaloID { get; set; }
        public Felhasznalo? Felhasznalo { get; set; }

        public string? Cim { get; set; }
        public string? Tartalom { get; set; }
        public string? KepUrl { get; set; }

        public DateTime Letrehozva { get; set; } = DateTime.UtcNow;

        public int? SpotID { get; set; }
        public Spot? Spot { get; set; }

        public ICollection<Komment> Kommentek { get; set; } = new List<Komment>();
    }
}