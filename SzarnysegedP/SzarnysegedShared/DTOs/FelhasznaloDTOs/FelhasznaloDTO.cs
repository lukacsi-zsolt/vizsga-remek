namespace SzarnysegedShared.DTOs.FelhasznaloDTOs
{
    public class FelhasznaloDTO
    {
        public int FelhasznaloID { get; set; }
        public string? FelhasznaloNev { get; set; }
        public string? TeljesNev { get; set; }
        public string? Email { get; set; }
        public DateTime? SzuletesiDatum { get; set; }
        public string? Bio { get; set; }
        public string? Helyszin { get; set; }
        public string? Klub { get; set; }
        public string? AvatarUrl { get; set; }
        public string? CoverUrl { get; set; }
        public bool IsAdmin { get; set; }
        //jelszo hash meg mindig ne keruljon ide
    }
}