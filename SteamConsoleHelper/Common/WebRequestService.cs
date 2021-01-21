using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using SteamConsoleHelper.ApiModels.Responses;
using SteamConsoleHelper.Exceptions;

namespace SteamConsoleHelper.Common
{
    // ReSharper disable PossibleMultipleEnumeration
    // ReSharper disable PossibleNullReferenceException
    public class WebRequestService
    {
        private readonly HttpClientFactory _httpClientFactory;

        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Formatting = Formatting.Indented
        };

        public WebRequestService(HttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task GetRequestAsync(string url, IEnumerable<(string name, string value)> parameters = null)
        {
            var getUrl = url;

            if (parameters != null && parameters.Any())
            {
                getUrl += "?" + string.Join("&", parameters.Select(x => $"{x.name}={x.value}"));
            }

            using var httpClient = await _httpClientFactory.CreateAsync();
            var response = await httpClient.GetAsync(getUrl);

            if (!response.IsSuccessStatusCode)
            {
                throw new InternalException(InternalError.RequestBadRequest);
            }
        }

        public async Task<T> GetRequestAsync<T>(string url, IEnumerable<(string name, string value)> parameters = null)
            where T : SteamResponseBase
        {
            var getUrl = url;

            if (parameters != null && parameters.Any())
            {
                getUrl += "?" + string.Join("&", parameters.Select(x => $"{x.name}={x.value}"));
            }

            using var httpClient = await _httpClientFactory.CreateAsync();
            var response = await httpClient.GetAsync(getUrl);
            
            return await DeserializeResponseAsync<T>(response);
        }

        public async Task<T> PostRequestAsync<T>(string url, object data)
            where T : SteamResponseBase
        {
            var contentToPush = GetFormContent();

            using var httpClient = await _httpClientFactory.CreateAsync();
            var response = await httpClient.PostAsync(url, new FormUrlEncodedContent(contentToPush));

            return await DeserializeResponseAsync<T>(response);

            IEnumerable<KeyValuePair<string, string>> GetFormContent()
            {
                foreach (var prop in data.GetType().GetProperties())
                {
                    yield return new KeyValuePair<string, string>(prop.Name.ToLower(), prop.GetValue(data, null).ToString());
                }
            }
        }

        public async Task PostRequestAsync(string url, object data)
        {
            var contentToPush = GetFormContent();

            using var httpClient = await _httpClientFactory.CreateAsync();
            var response = await httpClient.PostAsync(url, new FormUrlEncodedContent(contentToPush));

            if (!response.IsSuccessStatusCode)
            {
                throw new InternalException(InternalError.RequestBadRequest);
            }

            IEnumerable<KeyValuePair<string, string>> GetFormContent()
            {
                foreach (var prop in data.GetType().GetProperties())
                {
                    yield return new KeyValuePair<string, string>(prop.Name.ToLower(), prop.GetValue(data, null).ToString());
                }
            }
        }

        private static async Task<T> DeserializeResponseAsync<T>(HttpResponseMessage response)
        where T : SteamResponseBase
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new InternalException(InternalError.RequestBadRequest);
            }

            var responseText = await response.Content.ReadAsStringAsync();

            try
            {
                var result = JsonConvert.DeserializeObject<T>(responseText, JsonSerializerSettings);

                if (result == null)
                {
                    throw new InternalException(InternalError.FailedToDeserializeResponse);
                }

                if (!result.Success)
                {
                    throw new InternalException(InternalError.FailActionResult, result.ErrorMessage);
                }

                return result;
            }
            catch (InternalException)
            {
                throw;
            }
            catch (Exception e)
            {
                var text = $"responseText: '{responseText}'; {e}";
                Console.WriteLine(text);
                throw new InternalException(InternalError.FailedToDeserializeResponse, text);
            }
        }
    }
}