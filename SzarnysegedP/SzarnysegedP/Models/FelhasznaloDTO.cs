using System.Text.Json.Serialization;

namespace SzarnysegedP.Models
{
    public class FelhasznaloDTO
    {
        [JsonPropertyName("FelhasznaloID")]
        public int FelhasznaloID { get; set; }
        [JsonPropertyName("FelhasznaloNev")]
        public string? FelhasznaloNev { get; set; }
        [JsonPropertyName("TeljesNev")]
        public string? TeljesNev { get; set; }
        [JsonPropertyName("Email")]
        public string? Email { get; set; }
        [JsonPropertyName("SzuletesiDatum")]
        public DateTime? SzuletesiDatum { get; set; }
    }
}
