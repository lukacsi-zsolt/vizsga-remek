using System.Text.Json.Serialization;

namespace SzarnysegedP.Models
{
    public class UtvonalDTO
    {
        [JsonPropertyName("UtvonalID")]
        public int UtvonalID { get; set; }
        [JsonPropertyName("FelhasznaloID")]
        public int FelhasznaloID { get; set; }
        [JsonPropertyName("SpotID")]
        public int SpotID { get; set; }
        [JsonPropertyName("IndulasIdo")]
        public DateTime? IndulasIdo { get; set; }
        [JsonPropertyName("ErkezesIdo")]
        public DateTime? ErkezesIdo { get; set; }
        [JsonPropertyName("TavolsagKM")]
        public double? TavolsagKM { get; set; }
        [JsonPropertyName("Megjegyzes")]
        public string Megjegyzes { get; set; }
    }
}
