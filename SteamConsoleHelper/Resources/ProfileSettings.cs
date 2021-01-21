using System.Net.Http;
using System.Threading.Tasks;

namespace SteamConsoleHelper.Resources
{
    public class ProfileSettings
    {
        private const string DirectProfileUrl = "https://steamcommunity.com/profiles/{0}/";

        public string SteamId { get; }

        // todo: somehow get it
        public string SteamUrlNickname { get; } = "valentaiga";

        private string _cachedProfileUrl = "https://steamcommunity.com/id/valentaiga/"; // for tests its constant

        public PrivateInformation PrivateTokens { get; }

        public ProfileSettings()
        {
            SteamId = "76561198038053448";
            PrivateTokens = new PrivateInformation("9c7b528f6163064bc0f45b2b",
                "76561198038053448%7C%7CB280C6CDF0E82633E30A18BD9EDA0F1EAAFE6997");
        }

        public async ValueTask<string> GetProfileUrlAsync()
        {
            if (_cachedProfileUrl != null)
            {
                return _cachedProfileUrl;
            }

            var url = string.Format(DirectProfileUrl, SteamId);
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url);

            _cachedProfileUrl = response.RequestMessage.RequestUri.ToString();
            return _cachedProfileUrl;
        }

        public class PrivateInformation
        {
            // todo: somehow change it for different users
            public string SessionId { get; }

            public string SteamLoginSecure { get; }
            
            public PrivateInformation(string sessionId, string steamLoginSecure)
                => (SessionId, SteamLoginSecure) = (sessionId, steamLoginSecure);
        }
    }
}