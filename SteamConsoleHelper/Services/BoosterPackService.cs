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
        private readonly ProfileSettings _profileSettings;

        public BoosterPackService(SteamUrlService steamUrlService, WebRequestService webRequestService, ProfileSettings profileSettings)
        {
            _steamUrlService = steamUrlService;
            _webRequestService = webRequestService;
            _profileSettings = profileSettings;
        }

        public async Task<List<BoosterCard>> UnpackBooster(uint appId, ulong assetId)
        {
            var url = _steamUrlService.UnpackBoosterUrl();

            var data = new UnpackBoosterPostModel(
                _profileSettings.SessionId,
                appId,
                assetId);

            var response = await _webRequestService.PostRequestAsync<UnpackBoosterResponseModel>(url, data);
            return response.Cards.Select(x => x.ToModel()).ToList();
        }

        public async Task CraftBooster(uint appId)
        {
            var url = _steamUrlService.CreateBoosterUrl();

            var data = new CreateBoosterPostModel(
                _profileSettings.SessionId,
                appId);

            await _webRequestService.PostRequestAsync<SteamResponseBase>(url, data);
        }
    }
}