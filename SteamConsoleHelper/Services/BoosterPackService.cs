using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using SteamConsoleHelper.Abstractions.BoosterPack;
using SteamConsoleHelper.ApiModels.Requests;
using SteamConsoleHelper.ApiModels.Responses;
using SteamConsoleHelper.ApiModels.Responses.BoosterPack;
using SteamConsoleHelper.Common;
using SteamConsoleHelper.Helpers;
using SteamConsoleHelper.Resources;

namespace SteamConsoleHelper.Services
{
    public class BoosterPackService
    {
        private readonly SteamUrlService _steamUrlService;
        private readonly WebRequestService _webRequestService;

        public BoosterPackService(SteamUrlService steamUrlService, WebRequestService webRequestService)
        {
            _steamUrlService = steamUrlService;
            _webRequestService = webRequestService;
        }

        public async Task<List<BoosterCard>> UnpackBoosterAsync(uint appId, ulong assetId)
        {
            var url = _steamUrlService.UnpackBoosterUrl();

            var data = new UnpackBoosterPostModel(
                Settings.SessionId,
                appId,
                assetId);

            var response = await _webRequestService.PostRequestAsync<UnpackBoosterResponseModel>(url, data);
            return response.Cards.Select(x => x.ToModel()).ToList();
        }

        public async Task CraftBoosterAsync(uint appId)
        {
            var url = _steamUrlService.CreateBoosterUrl();

            var data = new CreateBoosterPostModel(
                Settings.SessionId,
                appId);

            await _webRequestService.PostRequestAsync<SteamResponseBase>(url, data);
        }
    }
}