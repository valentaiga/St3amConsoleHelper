using Newtonsoft.Json;

namespace SteamConsoleHelper.ApiModels.Responses
{
    public class SteamResponseBase
    {
        [JsonIgnore]
        public bool Success => SuccessField == "true" || SuccessField == "1";

        [JsonProperty("success")]
        private string SuccessField { get; set; }

        [JsonProperty("message")]
        public string ErrorMessage { get; set; }
    }
}