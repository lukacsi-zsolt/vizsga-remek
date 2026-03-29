namespace ErnyosKozoApi.Models
{
    public class Felhasznalo
    {
        public int FelhasznaloID { get; set; }
        public string? FelhasznaloNev { get; set; }
        public string? TeljesNev { get; set; }
        public string? Email { get; set; }
        public DateTime? SzuletesiDatum { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime? RegDatum { get; set; } = DateTime.UtcNow;

        public string? Bio { get; set; }
        public string? Helyszin { get; set; }
        public string? Klub { get; set; }
        public string? AvatarUrl { get; set; }
        public string? CoverUrl { get; set; }

        public bool IsAdmin { get; set; } = false;
    }
}