using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using SteamConsoleHelper.Abstractions.Cache;
using SteamConsoleHelper.Abstractions.Inventory;
using SteamConsoleHelper.Abstractions.Market;
using SteamConsoleHelper.Extensions;
using SteamConsoleHelper.Helpers;
using SteamConsoleHelper.Services;

namespace SteamConsoleHelper.BackgroundServices.ScheduledJobs
{
    public class CheckMarketPriceJob : ScheduledJobBase
    {
        private static readonly TimeSpan DefaultRequestDelay = TimeSpan.FromSeconds(3);

        private readonly LocalCacheService _cacheService;
        private readonly MarketService _marketService;
        private readonly DelayedExecutionPool _delayedExecutionPool;

        public CheckMarketPriceJob(LocalCacheService cacheService, MarketService marketService, DelayedExecutionPool delayedExecutionPool, JobManager jobManager)
            : base(jobManager)
        {
            _cacheService = cacheService;
            _marketService = marketService;
            _delayedExecutionPool = delayedExecutionPool;

            JobExecuteDelay = TimeSpan.FromHours(1);
        }

        public override async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            // todo: add to cache model 'DateTime:sentToMarketTime' and ignore all listings earlier than 3 days
            var notSoldItems = await GetNotSoldItemsAsync();

            // todo: move some constants to GlobalSettings.cs (like timespan with default delay)
            foreach (var notSoldItem in notSoldItems)
            {
                await Task.Delay(DefaultRequestDelay);

                var myPrice = notSoldItem.itemWithPrice.Price;
                var lowestMarketPrice = await GetLowestMarketPriceAsync(notSoldItem.itemWithPrice.Item);
                var calculatedMarketPrice = PriceHelper.CalculateSellerPrice(lowestMarketPrice);

                if (myPrice > calculatedMarketPrice)
                {
                    _delayedExecutionPool.EnqueueRequestToPool(async () =>
                    {
                        await _marketService.RemoveItemFromListing(notSoldItem.listing.ListingId);
                    });
                }
            }
        }

        private async Task<List<(ItemWithPrice itemWithPrice, MarketListing listing)>> GetNotSoldItemsAsync()
        {
            var marketListings = await _marketService.GetAllMyListings();
            var sentToMarketItems = await _cacheService.GetCachedSentItemsToMarket();

            var mappedListings = InventoryHelper.MapPrices(sentToMarketItems, marketListings);

            // filter:
            // 1. already sold items
            // 2. items older than 3 days
            var maximumDateForCheck = DateTime.UtcNow.AddDays(-3);
            var soldItems = mappedListings
                .FindAll(x => x.listing != null)
                .FindAll(x => x.itemWithPrice.SellTime < maximumDateForCheck);
            foreach (var soldItem in soldItems)
            {
                await _cacheService.RemoveSentItemToMarketFromCacheAsync(
                    soldItem.itemWithPrice.Item,
                    soldItem.itemWithPrice.Price);
            }

            var notSoldItems = mappedListings.FindAll(x => x.listing != null);

            return notSoldItems;
        }

        private async Task<uint?> GetLowestMarketPriceAsync(InventoryItem item)
        {
            var itemPrice = await _marketService.GetItemPriceAsync(item);
            return itemPrice.LowestPrice;
        }
    }
}