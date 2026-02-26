using System.ComponentModel.DataAnnotations;

namespace SzarnysegedShared.DTOs.FelhasznaloDTOs
{
    public class CreateFelhazsnaloDto
    {
        [Required]
        public string? FelhasznaloNev { get; set; }

        [Required]
        public string? TeljesNev { get; set; }
        [Required]
        public string? Email { get; set; }
        [Required]
        public DateTime? SzuletesiDatum { get; set; }
    }
}