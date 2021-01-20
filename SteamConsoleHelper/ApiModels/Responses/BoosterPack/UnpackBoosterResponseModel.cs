using System.Collections.Generic;

using Newtonsoft.Json;

namespace SteamConsoleHelper.ApiModels.Responses.BoosterPack
{
    public class UnpackBoosterResponseModel : SteamResponseBase
    {
        [JsonProperty("rgItems")]
        public List<BoosterCardResponseModel> Cards { get; set; }
    }
}