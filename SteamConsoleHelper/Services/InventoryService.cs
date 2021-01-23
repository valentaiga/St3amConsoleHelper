using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SteamConsoleHelper.Abstractions;
using SteamConsoleHelper.Abstractions.Inventory;
using SteamConsoleHelper.ApiModels.Requests;
using SteamConsoleHelper.ApiModels.Responses;
using SteamConsoleHelper.ApiModels.Responses.Inventory;
using SteamConsoleHelper.Common;
using SteamConsoleHelper.Helpers;
using SteamConsoleHelper.Resources;

namespace SteamConsoleHelper.Services
{
    public class InventoryService
    {
        private readonly ProfileSettings _profileSettings;
        private readonly SteamUrlService _steamUrlService;
        private readonly WebRequestService _requestService;

        public InventoryService(ProfileSettings profileSettings, SteamUrlService steamUrlService, WebRequestService requestService)
        {
            _profileSettings = profileSettings;
            _steamUrlService = steamUrlService;
            _requestService = requestService;
        }

        public async Task GrindItemIntoGooAsync(InventoryItem item)
            => await GrindItemIntoGooAsync(item.AppId, item.AssetId, item.ContextId);

        public async Task GrindItemIntoGooAsync(uint appId, ulong assetId, uint contextId)
        {
            var url = _steamUrlService.GrindItemIntoGooUrl();
            var data = new GrindItemIntoGemsPostModel(
                _profileSettings.SessionId,
                appId,
                assetId,
                contextId);

            await _requestService.PostRequestAsync<SteamResponseBase>(url, data);
        }

        public async Task<List<InventoryItem>> GetInventoryAsync()
        {
            var url = _steamUrlService.GetCurrentInventoryUrl();
            var responseModel = await _requestService.GetRequestAsync<InventoryResponseModel>(url);

            var result = InventoryHelper.MapAssets(responseModel.Assets, responseModel.Descriptions)
                .Select(x => x.asset.ToModel(x.description))
                .ToList();

            return result;
        }
    }
}