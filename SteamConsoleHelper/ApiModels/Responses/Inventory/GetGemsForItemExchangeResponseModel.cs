using Newtonsoft.Json;

namespace SteamConsoleHelper.ApiModels.Responses.Inventory
{
    public class GetGemsForItemExchangeResponseModel : SteamResponseBase
    {
        [JsonProperty("goo_value")]
        public uint GooValue { get; set; }

        [JsonProperty("item_appid")]
        public ulong AppId { get; set; }

        [JsonProperty("item_type")]
        public uint ItemType { get; set; }
    }
}