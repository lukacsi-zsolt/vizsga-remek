using System.ComponentModel.DataAnnotations;

namespace SzarnysegedShared.Dtos
{
    public class CreateHirDto
    {
        [Required]
        public string? Cim { get; set; }

        [Required]
        public string? Tartalom { get; set; }

        public string? KepUrl { get; set; }
        public string? Kategoria { get; set; }

        public DateTime Datum { get; set; } = DateTime.Now;
    }
}