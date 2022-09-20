using EDVTrader.API;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EDVTrader.Common
{
    public class Config
    {
        [JsonPropertyName("category")]
        public int Category { get; set; }

        [JsonPropertyName("commodity")]
        public int Commodity { get; set; }

        [JsonPropertyName("ship_size")]
        public ShipSize ShipSize { get; set; }

        [JsonPropertyName("max_distance")]
        public int MaxDistance { get; set; }

        [JsonPropertyName("use_fleet_carriers")]
        public bool UseFleetCarriers { get; set; }

        [JsonPropertyName("use_planetary_stations")]
        public bool UsePlanetaryStations { get; set; }

        [JsonPropertyName("selected_system")]
        public string? SelectedSystem { get; set; }

        public static Config? Read()
        {
            if (!Directory.Exists("Data"))
                return null;

            if (!File.Exists("Data/Previous.json"))
                return null;

            return JsonSerializer.Deserialize<Config>(File.ReadAllText("Data/Previous.json"));
        }

        public void Save()
        {
            if (!Directory.Exists("Data"))
                Directory.CreateDirectory("Data");

            File.WriteAllText("Data/Previous.json", JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
