namespace ErnyosKozoApi.Models
{
    public class Spot
    {
        public int SpotID { get; set; }
        public string? Nev { get; set; }
        public string? Slug { get; set; }
        public string? Orszag { get; set; }
        public string? Megye { get; set; }
        public string? HelyLeiras { get; set; }
        public int? Magassag { get; set; }
        public double? AtlagSzel { get; set; }
        public string? Szabalyok { get; set; }

        public double? Lat { get; set; }
        public double? Lon { get; set; }

        public int? LetrehozoFelhasznaloID { get; set; }
    }
}