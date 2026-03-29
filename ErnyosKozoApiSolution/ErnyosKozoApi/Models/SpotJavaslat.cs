namespace ErnyosKozoApi.Models
{
    public class SpotJavaslat
    {
        public int SpotJavaslatID { get; set; }

        public string? Nev { get; set; }
        public string? Orszag { get; set; }
        public string? Megye { get; set; }
        public string? HelyLeiras { get; set; }
        public int? Magassag { get; set; }
        public double? AtlagSzel { get; set; }
        public string? Szabalyok { get; set; }
        public double? Lat { get; set; }
        public double? Lon { get; set; }

        public int? BekuldoFelhasznaloID { get; set; }
        public DateTime Letrehozva { get; set; } = DateTime.UtcNow;

        public bool Feldolgozva { get; set; } = false;
        public string? AdminMegjegyzes { get; set; }
    }
}