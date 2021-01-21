using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using SteamConsoleHelper.Abstractions.Inventory;
using SteamConsoleHelper.Abstractions.Market;
using SteamConsoleHelper.ApiModels.Requests;
using SteamConsoleHelper.ApiModels.Responses;
using SteamConsoleHelper.ApiModels.Responses.Market;
using SteamConsoleHelper.Common;
using SteamConsoleHelper.Extensions;
using SteamConsoleHelper.Helpers;
using SteamConsoleHelper.Resources;

namespace SteamConsoleHelper.Services
{
    public class MarketService
    {
        private readonly WebRequestService _requestService;
        private readonly SteamUrlService _steamUrlService;
        private readonly ProfileSettings _profileSettings;
        private readonly LocalCacheService _localCacheService;

        public MarketService(WebRequestService requestService, SteamUrlService steamUrlService, ProfileSettings profileSettings, LocalCacheService localCacheService)
        {
            _requestService = requestService;
            _steamUrlService = steamUrlService;
            _profileSettings = profileSettings;
            _localCacheService = localCacheService;
        }

        public async Task<ItemMarketPrice> GetItemPriceAsync(InventoryItem item)
            => await GetItemPriceAsync(item.AppId, item.MarketHashName);

        public async Task<ItemMarketPrice> GetItemPriceAsync(uint appId, string hashName)
        {
            var url = _steamUrlService.GetItemPriceUrl(appId, hashName);

            var response = await _requestService.GetRequestAsync<ItemPriceResponseModel>(url);

            Console.WriteLine($"{nameof(MarketService)}: Got lowest price '{response.LowestPriceString}' for '{hashName}'");

            return response.ToModel(appId, hashName);
        }

        /// <summary>
        /// Sell item on market (requires confirmation)
        /// </summary>
        /// <param name="item">Item to sell.</param>
        /// <param name="price">Price (85% of original market price).</param>
        /// <returns></returns>
        public async Task SellItemAsync(InventoryItem item, uint price)
        {
            var url = _steamUrlService.SellItemUrl();
            var data = new SellItemPostModel(
                _profileSettings.PrivateTokens.SessionId,
                item.AppId,
                item.AssetId,
                item.ContextId,
                price);

            await _requestService.PostRequestAsync<SteamResponseBase>(url, data);
            await _localCacheService.AddSentItemToMarketToCacheAsync(item, price);
            Console.WriteLine($"{nameof(MarketService)}: Sent sell request for assetId:'{item.AssetId}' with price '{price}'");
        }

        public async Task RemoveItemFromListing(ulong listingId)
        {
            var url = _steamUrlService.RemoveListingUrl(listingId);
            var data = new RemoveListingPostModel(_profileSettings.PrivateTokens.SessionId);

            await _requestService.PostRequestAsync(url, data);
            Console.WriteLine($"{nameof(MarketService)}: Removed listing from market");
        }

        public async Task<List<MarketListing>> GetAllMyListings()
        {
            uint offset = 0;
            var url = _steamUrlService.GetMyListingsUrl(offset);

            var response = await _requestService.GetRequestAsync<MyListingsResponseModel>(url);
            var responses = new List<MyListingsResponseModel> { response };

            while (!response.IsTheEnd)
            {
                offset += 100;
                url = _steamUrlService.GetMyListingsUrl(offset);
                await Task.Delay(TimeSpan.FromSeconds(3));
                response = await _requestService.GetRequestAsync<MyListingsResponseModel>(url);
                responses.Add(response);
            }

            var result = new List<MarketListing>();

            foreach (var resp in responses)
            {
                var hovers = ParseHelper.ParseListingHover(resp.Hovers);

                var listings = resp.Assets.Zip(hovers)
                    .Select(x => x.First.ToModel(x.Second))
                    .ToList();

                result.AddRange(listings);
            }

            return result;
        }
    }
}