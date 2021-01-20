
using Newtonsoft.Json;

namespace SteamConsoleHelper.ApiModels.Responses.Market
{
    public class ItemPriceResponseModel : SteamResponseBase
    {
        public string Volume { get; set; }

        [JsonProperty("lowest_price")]
        public string LowestPriceString { get; set; }

        [JsonProperty("median_price")]
        public string MedianPriceString { get; set; }
    }
}