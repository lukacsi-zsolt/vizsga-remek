namespace SzarnysegedShared.DTOs
{
    public class HirDto
    {
        public int HirID { get; set; }
        public string? Cim { get; set; }
        public string? Tartalom { get; set; }
        public string? KepUrl { get; set; }
        public string? Kategoria { get; set; }
        public DateTime Datum { get; set; }
    }
}