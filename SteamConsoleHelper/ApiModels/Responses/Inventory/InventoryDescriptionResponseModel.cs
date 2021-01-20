using System.Collections.Generic;

using Newtonsoft.Json;

namespace SteamConsoleHelper.ApiModels.Responses.Inventory
{
    public class InventoryDescriptionResponseModel
    {
        public uint AppId { get; set; }

        public ulong ClassId { get; set; }

        public ulong InstanceId { get; set; }

        public uint Currency { get; set; }

        [JsonProperty("icon_url")]
        public string IconUrl { get; set; }

        public bool Tradable { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        [JsonProperty("market_name")]
        public string MarketName { get; set; }

        [JsonProperty("market_hash_name")]
        public string MarketHashName { get; set; }

        [JsonProperty("market_fee_app")]
        public uint MarketFeeApp { get; set; }

        public bool Commodity { get; set; }

        [JsonProperty("market_tradable_restriction")]
        public uint MarketTradableRestriction { get; set; }

        [JsonProperty("market_marketable_restriction")]
        public uint MarketMarketableRestriction { get; set; }

        public bool Marketable { get; set; }

        [JsonProperty("tags")]
        public List<ItemTagResponseModel> Tags { get; set; }
    }
}