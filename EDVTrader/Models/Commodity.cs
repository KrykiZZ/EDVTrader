using System.Text.Json.Serialization;

namespace EDVTrader.Models
{
    public class Commodity
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("category_id")]
        public int CategoryId { get; set; }

        [JsonPropertyName("average_price")]
        public int? AveragePrice { get; set; }

        [JsonPropertyName("is_rare")]
        public int IsRare { get; set; }

        [JsonPropertyName("max_buy_price")]
        public int? MaxBuyPrice { get; set; }

        [JsonPropertyName("max_sell_price")]
        public int? MaxSellPrice { get; set; }

        [JsonPropertyName("min_buy_price")]
        public int? MinBuyPrice { get; set; }

        [JsonPropertyName("min_sell_price")]
        public int? MinSellPrice { get; set; }

        [JsonPropertyName("buy_price_lower_average")]
        public int BuyPriceLowerAverage { get; set; }

        [JsonPropertyName("sell_price_upper_average")]
        public int SellPriceUpperAverage { get; set; }

        [JsonPropertyName("is_non_marketable")]
        public int IsNonMarketable { get; set; }

        [JsonPropertyName("ed_id")]
        public int EdId { get; set; }

        [JsonPropertyName("category")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public CommodityCategory Category { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    
        public int InaraId { get; set; }
    }
}
