using System.Collections.Generic;
using System.Linq;

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

        public string Hovers { get; set; }

        [JsonIgnore]
        public List<ListingAsset> Assets
            => InnerAssets
                .SelectMany(x => x.Value)
                .SelectMany(x => x.Value)
                .Select(x => x.Value)
                .ToList();

        [JsonProperty("assets")]
        private Dictionary<string, Dictionary<string, Dictionary<string, ListingAsset>>> InnerAssets { get; set; }

        [JsonIgnore]
        public bool IsTheEnd => Offset != 0
            ? TotalCount == ActiveListingsCount
            : TotalCount == Offset + ActiveListingsCount;
    }
}