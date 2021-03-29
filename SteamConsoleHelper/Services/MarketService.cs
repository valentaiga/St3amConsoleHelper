using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using SteamConsoleHelper.Abstractions.Inventory;
using SteamConsoleHelper.Abstractions.Market;
using SteamConsoleHelper.ApiModels.Requests;
using SteamConsoleHelper.ApiModels.Responses;
using SteamConsoleHelper.ApiModels.Responses.Market;
using SteamConsoleHelper.Common;
using SteamConsoleHelper.Helpers;
using SteamConsoleHelper.Resources;

namespace SteamConsoleHelper.Services
{
    public class MarketService
    {
        private readonly ILogger<MarketService> _logger;
        private readonly WebRequestService _requestService;
        private readonly SteamUrlService _steamUrlService;

        public MarketService(
            ILogger<MarketService> logger,
            WebRequestService requestService, 
            SteamUrlService steamUrlService)
        {
            _logger = logger;
            _requestService = requestService;
            _steamUrlService = steamUrlService;
        }

        public async Task<ItemMarketPrice> GetItemPriceAsync(InventoryItem item)
            => await GetItemPriceAsync(item.AppId, item.MarketHashName);

        public async Task<ItemMarketPrice> GetItemPriceAsync(uint appId, string hashName)
        {
            var url = _steamUrlService.GetItemPriceUrl(appId, hashName);

            var response = await _requestService.GetRequestAsync<ItemPriceResponseModel>(url);

            _logger.LogDebug($"Got lowest price '{response.LowestPriceString}' for '{hashName}'");

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
                Settings.SessionId,
                item.AppId,
                item.AssetId,
                item.ContextId,
                price);

            await _requestService.PostRequestAsync<SteamResponseBase>(url, data);
            _logger.LogInformation($"Sent to market item '{item.MarketHashName}' assetId: '{item.AssetId}' with price '{price}'");
        }

        public async Task RemoveItemFromListingAsync(ulong listingId)
        {
            var url = _steamUrlService.RemoveListingUrl(listingId);
            var data = new RemoveListingPostModel(Settings.SessionId);

            await _requestService.PostRequestAsync(url, data);
            _logger.LogInformation($"Removed listing from market assetId '{listingId}'");
        }

        public async Task<List<MarketListing>> GetAllMyListingsAsync()
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
                var descriptions = ParseHelper.ParseHtmlResult(resp.HtmlResult);

                var listings = resp.Assets.Zip(hovers)
                    .Select(x => x.First.ToModel(x.Second, descriptions.Find(d => d.ListingId == x.Second.ListingId)))
                    .ToList();

                result.AddRange(listings);
            }

            _logger.LogInformation($"Total '{result.Count}' items on market, '{result.FindAll(x => x.AwaitingConfirmation).Count}' of them awaiting confirmation");

            return result;
        }

        public async Task CreateItemBuyOrderAsync(uint appId, string marketHashName, uint price, uint quantity)
        {
            var url = _steamUrlService.CreateBuyOrderUrl();
            var data = new CreateBuyOrderPostModel(Settings.SessionId, appId, marketHashName, price, quantity);

            await _requestService.PostRequestAsync<SteamResponseBase>(url, data);
            _logger.LogInformation($"Buy order for '{marketHashName}' created with price {price}");
        }

        public async Task CancelItemBuyOrderAsync(ulong orderId)
        {
            var url = _steamUrlService.CancelBuyOrderUrl();
            var data = new CancelBuyOrderPostModel(Settings.SessionId, orderId);

            await _requestService.PostRequestAsync(url, data);
            _logger.LogInformation($"Buy order with id '{orderId}' canceled");
        }
    }
}