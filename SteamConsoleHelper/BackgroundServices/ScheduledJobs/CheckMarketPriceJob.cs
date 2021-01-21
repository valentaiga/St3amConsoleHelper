using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using SteamConsoleHelper.Abstractions.Market;
using SteamConsoleHelper.Services;

namespace SteamConsoleHelper.BackgroundServices.ScheduledJobs
{
    /// <summary>
    /// Job for removing more expensive items from market
    /// </summary>
    public class CheckMarketPriceJob : ScheduledJobBase<CheckMarketPriceJob>
    {
        private static readonly TimeSpan DefaultRequestDelay = TimeSpan.FromSeconds(3);
        private const uint ExpensivePrice = 100_00;

        private readonly ILogger<CheckMarketPriceJob> _logger;
        private readonly LocalCacheService _cacheService;
        private readonly MarketService _marketService;
        private readonly DelayedExecutionPool _delayedExecutionPool;

        public CheckMarketPriceJob(
            ILogger<CheckMarketPriceJob> logger,
            LocalCacheService cacheService,
            MarketService marketService,
            DelayedExecutionPool delayedExecutionPool,
            JobManager jobManager)
            : base(logger, jobManager)
        {
            _logger = logger;
            _cacheService = cacheService;
            _marketService = marketService;
            _delayedExecutionPool = delayedExecutionPool;

            JobExecuteDelay = TimeSpan.FromMinutes(15);
        }

        public override async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            // todo: add to cache model 'DateTime:sentToMarketTime' and ignore all listings earlier than 3 days
            var notSoldItems = await GetNotSoldItemsAsync();

            // todo: move some constants to GlobalSettings.cs (like timespan with default delay)
            foreach (var notSoldItem in notSoldItems)
            {
                await Task.Delay(DefaultRequestDelay, cancellationToken);
                
                _logger.LogDebug($"listingId: '{notSoldItem.ListingId}' hashName: {notSoldItem.HashName}");
                var lowestMarketPrice = await GetLowestMarketPriceAsync(notSoldItem.AppId, notSoldItem.HashName);

                if (notSoldItem.BuyerPrice > lowestMarketPrice)
                {
                    _delayedExecutionPool.EnqueueActionToPool(async () =>
                    {
                        await _marketService.RemoveItemFromListing(notSoldItem.ListingId);
                    });
                }
            }
        }

        private async Task<List<MarketListing>> GetNotSoldItemsAsync()
        {
            var marketListings = await _marketService.GetAllMyListings();

            // filter items older than 3 days OR price is too hugh and it needed to be change now 
            var maximumDateForCheck = DateTime.UtcNow.AddDays(-3);
            var result = marketListings
                .FindAll(x => x.SellDate < maximumDateForCheck || x.SellerPrice > ExpensivePrice);
            _logger.LogDebug($"Total '{result.Count}' items on market older than 3 days or price > 100 rubles");
            
            return result;
        }

        private async Task<uint?> GetLowestMarketPriceAsync(uint appId, string hashName)
        {
            var itemPrice = await _marketService.GetItemPriceAsync(appId, hashName);
            return itemPrice.LowestPrice;
        }
    }
}