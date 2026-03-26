using System.ComponentModel.DataAnnotations;

namespace SzarnysegedShared.DTOs.FelhasznaloDTOs
{
    public class UpdateFelhasznaloDto
    {
        [Required]
        public string FelhasznaloNev { get; set; } = string.Empty;

        [Required]
        public string TeljesNev { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public DateTime? SzuletesiDatum { get; set; }

        public string? Password { get; set; }

        public string? Bio { get; set; }
        public string? Helyszin { get; set; }
        public string? Klub { get; set; }
        public string? AvatarUrl { get; set; }
        public string? CoverUrl { get; set; }
    }
}