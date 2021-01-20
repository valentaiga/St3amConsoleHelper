using Newtonsoft.Json;

namespace SteamConsoleHelper.ApiModels.Responses.BoosterPack
{
    public class BoosterCardResponseModel
    {
        public string Name { get; set; }

        [JsonProperty("foil")]
        public bool IsFoil { get; set; }

        [JsonProperty("image")]
        public string ImageUrl { get; set; }

        [JsonProperty("series")]
        public uint Series { get; set; }
    }
}