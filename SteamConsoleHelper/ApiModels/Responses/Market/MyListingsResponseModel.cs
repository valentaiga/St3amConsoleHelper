using System.Collections.Generic;
using Newtonsoft.Json;
using SteamConsoleHelper.ApiModels.Responses.Inventory;

namespace SteamConsoleHelper.ApiModels.Responses.Market
{
    public class MyListingsResponseModel : SteamResponseBase
    {
        public uint PageSize { get; set; }

        [JsonProperty("total_count")]
        public uint TotalCount { get; set; }
        
        [JsonProperty("num_active_listings")]
        public uint ActiveListingsCount { get; set; }

        [JsonProperty("start")]
        public uint Offset { get; set; }

        public bool IsTheEnd => Offset != 0
            ? TotalCount == ActiveListingsCount
            : TotalCount == Offset + ActiveListingsCount;

        public List<InventoryAssetResponseModel> Assets { get; set; }
    }
}