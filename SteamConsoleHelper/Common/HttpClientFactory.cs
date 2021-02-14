using System;
using System.Net;
using System.Net.Http;

using SteamConsoleHelper.Resources;

namespace SteamConsoleHelper.Common
{
    public class HttpClientFactory
    {
        public HttpClient Create()
        {
            var inventoryUrl = Settings.ProfileUrl + "inventory";
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli,
                CookieContainer = new CookieContainer()
            };

            handler.CookieContainer.Add(new Cookie("sessionid", Settings.SessionId, "/", "steamcommunity.com"));
            handler.CookieContainer.Add(new Cookie("steamLoginSecure", Settings.SteamLoginSecure, "/", "steamcommunity.com"));

            var client = new HttpClient(handler);

            client.DefaultRequestHeaders.Accept.ParseAdd("*/*");
            client.DefaultRequestHeaders.Connection.Add("keep-alive");
            client.DefaultRequestHeaders.Add("DNT", "1");
            client.DefaultRequestHeaders.Referrer = new Uri(inventoryUrl);
            client.DefaultRequestHeaders.Host = "steamcommunity.com";
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.141 Safari/537.36");
            client.DefaultRequestHeaders.Add("Origin", "https://steamcommunity.com");
            client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "empty");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "cors");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-origin");

            return client;
        }
    }
}