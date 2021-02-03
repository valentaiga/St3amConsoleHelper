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
    // ReSharper disable PossibleNullReferenceException
    public class WebRequestService
    {
        private static bool EnableUrlLogs = false;

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
            if (!ProfileSettings.IsAuthenticated)
            {
                _logger.LogWarning("Request is failed, authentication tokens are invalid");
                throw new InternalException(InternalError.UserIsNotAuthenticated);
            }

            var getUrl = url;

            if (parameters != null && parameters.Any())
            {
                getUrl += "?" + string.Join("&", parameters.Select(x => $"{x.name}={x.value}"));
            }

            using var httpClient = _httpClientFactory.Create();
            var response = await httpClient.GetAsync(getUrl);
            if (EnableUrlLogs)
            {
                _logger.LogDebug($"GET statusCode: '{(int)response.StatusCode}' request: '{getUrl}'");
            }
            ValidateResponse(response);
        }

        public async ValueTask<T> GetRequestAsync<T>(string url, IEnumerable<(string name, string value)> parameters = null)
            where T : SteamResponseBase
        {
            if (!ProfileSettings.IsAuthenticated)
            {
                _logger.LogWarning("Request is failed, authentication tokens are invalid");
                throw new InternalException(InternalError.UserIsNotAuthenticated);
            }

            var getUrl = url;

            if (parameters != null && parameters.Any())
            {
                getUrl += "?" + string.Join("&", parameters.Select(x => $"{x.name}={x.value}"));
            }
            
            using var httpClient = _httpClientFactory.Create();
            var response = await httpClient.GetAsync(getUrl);

            if (EnableUrlLogs)
            {
                _logger.LogDebug($"GET statusCode: '{(int)response.StatusCode}' request '{getUrl}'");
            }
            return await DeserializeResponseAsync<T>(response);
        }

        public async ValueTask<T> PostRequestAsync<T>(string url, object data)
            where T : SteamResponseBase
        {
            if (!ProfileSettings.IsAuthenticated)
            {
                _logger.LogWarning("Request is failed, authentication tokens are invalid");
                throw new InternalException(InternalError.UserIsNotAuthenticated);
            }

            var contentToPush = GetFormContent();
            
            using var httpClient = _httpClientFactory.Create();
            var response = await httpClient.PostAsync(url, new FormUrlEncodedContent(contentToPush));

            if (EnableUrlLogs)
            {
                _logger.LogDebug($"POST statusCode: '{(int)response.StatusCode}' request '{url}'");
            }

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
            if (!ProfileSettings.IsAuthenticated)
            {
                _logger.LogWarning("Request is failed, authentication tokens are invalid");
                throw new InternalException(InternalError.UserIsNotAuthenticated);
            }

            var contentToPush = GetFormContent();
            
            using var httpClient = _httpClientFactory.Create();
            var response = await httpClient.PostAsync(url, new FormUrlEncodedContent(contentToPush));

            if (EnableUrlLogs)
            {
                _logger.LogDebug($"POST statusCode: '{(int)response.StatusCode}' request '{url}'");
            }

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
                    throw new InternalException(InternalError.UnexpectedError);
            }
        }
    }
}