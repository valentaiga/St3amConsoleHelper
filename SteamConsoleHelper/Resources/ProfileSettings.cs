using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Serilog;

using SteamAuth;

using SteamConsoleHelper.Exceptions;

namespace SteamConsoleHelper.Resources
{
    public class ProfileSettings
    {
        private const string DirectProfileUrl = "https://steamcommunity.com/profiles/{0}/";

        private readonly HttpClient _httpClient;

        public ProfileSettings()
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli,
                CookieContainer = new CookieContainer()
            };
            _httpClient = new HttpClient(handler);
        }

        public static bool IsAuthenticated { get; private set; }

        // todo: remove static and somehow update ProfileSettings in jobs
        public static UserLogin UserLogin { get; private set; }

        public static string UrlNickname { get; private set; }

        public static string ProfileUrl { get; private set; }

        public string SteamId => Environment.GetEnvironmentVariable("SteamId")
                                 ?? UserLogin?.Session.SteamID.ToString()
                                 ?? throw new InternalException(InternalError.UserIsNotAuthenticated);

        public string SessionId => Environment.GetEnvironmentVariable("SessionId")
                                          ?? UserLogin?.Session.SessionID
                                          ?? throw new InternalException(InternalError.UserIsNotAuthenticated);

        public string SteamLoginSecure => Environment.GetEnvironmentVariable("SteamLoginSecure")
                                          ?? UserLogin?.Session.SteamLoginSecure
                                          ?? throw new InternalException(InternalError.UserIsNotAuthenticated);

        public void SetUserLogin(UserLogin userLogin)
            => UserLogin = userLogin;

        public async Task InitializeAsync()
        {
            var url = string.Format(DirectProfileUrl, SteamId);

            var response = await _httpClient.GetAsync(url);

            ProfileUrl = response.RequestMessage.RequestUri.ToString();
            UrlNickname = ProfileUrl.Split('/', StringSplitOptions.RemoveEmptyEntries).Last();

            Log.Information($"Profile initialized: ProfileUrl: '{ProfileUrl}', UrlNickname: '{UrlNickname}'");
        }

        public static void SetIsAuthenticatedStatus(bool isAuthenticated)
            => IsAuthenticated = isAuthenticated;
    }
}