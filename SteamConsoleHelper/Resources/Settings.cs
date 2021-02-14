using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using SteamAuth;

using SteamConsoleHelper.Exceptions;

namespace SteamConsoleHelper.Resources
{
    /// <summary>
    /// Static settings for application
    /// </summary>
    public static class Settings
    {
        private const string DirectProfileUrl = "https://steamcommunity.com/profiles/{0}/";

        public static bool IsAuthenticated { get; private set; }
        
        public static UserLogin UserLogin { get; private set; }

        public static string UrlNickname { get; private set; }

        public static string ProfileUrl { get; private set; }

        public static string SteamId => UserLogin?.Session.SteamID.ToString() 
                                 ?? throw new InternalException(InternalError.UserIsNotAuthenticated);

        public static string SessionId => UserLogin?.Session.SessionID 
                                   ?? throw new InternalException(InternalError.UserIsNotAuthenticated);

        public static string SteamLoginSecure => UserLogin?.Session.SteamLoginSecure 
                                          ?? throw new InternalException(InternalError.UserIsNotAuthenticated);

        public static void SetUserLogin(UserLogin userLogin)
            => UserLogin = userLogin;

        public static async Task InitializeAsync()
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli,
                CookieContainer = new CookieContainer()
            };
            var httpClient = new HttpClient(handler);

            var url = string.Format(DirectProfileUrl, SteamId);

            var response = await httpClient.GetAsync(url);

            ProfileUrl = response.RequestMessage.RequestUri.ToString();
            UrlNickname = ProfileUrl.Split('/', StringSplitOptions.RemoveEmptyEntries).Last();
        }

        public static void SetIsAuthenticatedStatus(bool isAuthenticated)
            => IsAuthenticated = isAuthenticated;
    }
}