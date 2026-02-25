using System.Text.Json.Serialization;

namespace SzarnysegedP.Models
{
    public class SpotDTO
    {
        [JsonPropertyName("SpotID")]
        public int SpotID { get; set; }

        [JsonPropertyName("Nev")]
        public string? Nev { get; set; }

        [JsonPropertyName("Orszag")]
        public string? Orszag { get; set; }

        [JsonPropertyName("Megye")]
        public string? Megye { get; set; }

        [JsonPropertyName("HelyLeiras")]
        public string? HelyLeiras { get; set; }

        [JsonPropertyName("Magassag")]
        public int? Magassag { get; set; }

        [JsonPropertyName("AtlagSzel")]
        public double? AtlagSzel { get; set; }

        [JsonPropertyName("Szabalyok")]
        public string? Szabalyok { get; set; }
    }
}
