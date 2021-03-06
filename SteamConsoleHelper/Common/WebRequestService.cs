using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using SteamConsoleHelper.ApiModels.Responses;
using SteamConsoleHelper.Exceptions;
using SteamConsoleHelper.Resources;

namespace SteamConsoleHelper.Common
{
    // ReSharper disable PossibleMultipleEnumeration
    public class WebRequestService
    {
        private readonly ILogger<WebRequestService> _logger;
        private readonly HttpClientFactory _httpClientFactory;

        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Formatting = Formatting.Indented
        };

        public WebRequestService(ILogger<WebRequestService> logger, HttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async ValueTask GetRequestAsync(string url, IEnumerable<(string name, string value)> parameters = null)
        {
            CheckAuthenticatedStatus();

            var getUrl = url;

            if (parameters != null && parameters.Any())
            {
                getUrl += "?" + string.Join("&", parameters.Select(x => $"{x.name}={x.value}"));
            }

            using var httpClient = _httpClientFactory.Create();
            var response = await httpClient.GetAsync(getUrl);
            ValidateResponse(response);
        }

        public async ValueTask<string> DownloadPageAsync(string url)
        {
            CheckAuthenticatedStatus();

            using var httpClient = _httpClientFactory.Create();
            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new InternalException(InternalError.SteamServicesAreBusy);
            }

            return await response.Content.ReadAsStringAsync();
        }

        public async ValueTask<T> GetRequestAsync<T>(string url, IEnumerable<(string name, string value)> parameters = null)
            where T : SteamResponseBase
        {
            CheckAuthenticatedStatus();

            var getUrl = url;

            if (parameters != null && parameters.Any())
            {
                getUrl += "?" + string.Join("&", parameters.Select(x => $"{x.name}={x.value}"));
            }
            
            using var httpClient = _httpClientFactory.Create();
            var response = await httpClient.GetAsync(getUrl);
            
            return await DeserializeResponseAsync<T>(response);
        }

        public async ValueTask<T> PostRequestAsync<T>(string url, object data)
            where T : SteamResponseBase
        {
            CheckAuthenticatedStatus();

            var contentToPush = GetFormContent();
            
            using var httpClient = _httpClientFactory.Create();
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

        public async ValueTask PostRequestAsync(string url, object data)
        {
            CheckAuthenticatedStatus();

            var contentToPush = GetFormContent();
            
            using var httpClient = _httpClientFactory.Create();
            var response = await httpClient.PostAsync(url, new FormUrlEncodedContent(contentToPush));
            
            ValidateResponse(response);

            IEnumerable<KeyValuePair<string, string>> GetFormContent()
            {
                foreach (var prop in data.GetType().GetProperties())
                {
                    yield return new KeyValuePair<string, string>(prop.Name.ToLower(), prop.GetValue(data, null).ToString());
                }
            }
        }

        private async Task<T> DeserializeResponseAsync<T>(HttpResponseMessage response)
            where T : SteamResponseBase
        {
            ValidateResponse(response);

            var responseText = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<T>(responseText, JsonSerializerSettings);

            if (result == null)
            {
                throw new InternalException(InternalError.FailedToDeserializeResponse);
            }

            if (!result.Success)
            {
                throw new InternalException(InternalError.SteamServicesAreBusy, result.ErrorMessage);
            }

            return result;
        }

        private void ValidateResponse(HttpResponseMessage response)
        {
            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                case HttpStatusCode.Accepted:
                    return;
                case HttpStatusCode.BadRequest:
                    throw new InternalException(InternalError.RequestBadRequest);
                case HttpStatusCode.Unauthorized:
                    throw new InternalException(InternalError.RequestUnauthorized);
                case HttpStatusCode.Forbidden:
                    throw new InternalException(InternalError.TooManyRequests);
                case HttpStatusCode.NotFound:
                    throw new InternalException(InternalError.RequestNotFound);
                default:
                    _logger.LogError($"Unexpected error response: {response.StatusCode}:{response.ReasonPhrase}");
                    throw new InternalException(InternalError.UnexpectedError);
            }
        }

        private void CheckAuthenticatedStatus()
        {
            if (!Settings.IsAuthenticated)
            {
                _logger.LogWarning("Request is failed, authentication tokens are invalid");
                throw new InternalException(InternalError.UserIsNotAuthenticated);
            }
        }
    }
}