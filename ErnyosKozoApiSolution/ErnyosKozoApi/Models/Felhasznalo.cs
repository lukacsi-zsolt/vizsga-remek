namespace ErnyosKozoApi.Models
{
    public class Felhasznalo
    {
        public int FelhasznaloID { get; set; }
        public string? FelhasznaloNev { get; set; }
        public string? TeljesNev { get; set; }
        public string? Email { get; set; }
        public DateTime? SzuletesiDatum { get; set; }
        public string PasswordHash { get; set; }
    }

}
