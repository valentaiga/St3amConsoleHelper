using Newtonsoft.Json;

namespace SteamConsoleHelper.ApiModels.Responses
{
    public class SteamResponseBase
    {
        public uint Success { get; set; }

        [JsonProperty("message")]
        public string ErrorMessage { get; set; }
    }
}