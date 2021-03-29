using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

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
        private readonly ILogger<InventoryService> _logger;
        private readonly SteamUrlService _steamUrlService;
        private readonly WebRequestService _requestService;

        public InventoryService(ILogger<InventoryService> logger, SteamUrlService steamUrlService, WebRequestService requestService)
        {
            _logger = logger;
            _steamUrlService = steamUrlService;
            _requestService = requestService;
        }

        public async Task GrindItemIntoGooAsync(InventoryItem item)
            => await GrindItemIntoGooAsync(item.AppId, item.AssetId, item.ContextId);

        public async Task GrindItemIntoGooAsync(uint appId, ulong assetId, uint contextId)
        {
            var url = _steamUrlService.GrindItemIntoGemsUrl();
            var data = new GrindItemIntoGemsPostModel(
                Settings.SessionId,
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

            _logger.LogInformation($"Total items in inventory: '{result.Count}'");

            return result;
        }
    }
}