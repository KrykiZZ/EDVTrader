using System.Text.Json.Serialization;

namespace EDVTrader.Models
{
    public class CommodityCategory
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}
