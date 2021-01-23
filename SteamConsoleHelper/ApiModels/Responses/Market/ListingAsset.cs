using Newtonsoft.Json;

namespace SteamConsoleHelper.ApiModels.Responses.Market
{
    // ReSharper disable StringLiteralTypo
    public class ListingAsset
    {
        [JsonProperty("appid")]
        public uint AppId { get; set; }

        [JsonProperty("contextid")]
        public uint ContextId { get; set; }

        [JsonProperty("classid")]
        public ulong ClassId { get; set; }

        [JsonProperty("id")]
        public ulong AssetId { get; set; }
    }
}