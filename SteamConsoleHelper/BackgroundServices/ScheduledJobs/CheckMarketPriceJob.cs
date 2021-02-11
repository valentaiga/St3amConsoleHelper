using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using SteamConsoleHelper.Abstractions.Market;
using SteamConsoleHelper.Helpers;
using SteamConsoleHelper.Services;

namespace SteamConsoleHelper.BackgroundServices.ScheduledJobs
{
    /// <summary>
    /// Job for removing expired items from market
    /// </summary>
    public class CheckMarketPriceJob : ScheduledJobBase<CheckMarketPriceJob>
    {
        private const int DaysCountBeforeListingExpire = 2;

        private readonly ILogger<CheckMarketPriceJob> _logger;
        private readonly MarketService _marketService;
        private readonly DelayedExecutionPool _delayedExecutionPool;

        public CheckMarketPriceJob(
            ILogger<CheckMarketPriceJob> logger,
            MarketService marketService,
            DelayedExecutionPool delayedExecutionPool,
            JobManager jobManager)
            : base(logger, jobManager)
        {
            _logger = logger;
            _marketService = marketService;
            _delayedExecutionPool = delayedExecutionPool;

            JobExecuteDelay = TimeSpan.FromMinutes(15);
        }

        public override async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            var notSoldItems = await GetNotSoldItemsAsync();

            // todo: move some constants to GlobalSettings.cs (like timespan with default delay)
            Parallel.ForEach(notSoldItems, (notSoldItem) => _delayedExecutionPool.EnqueueTaskToPool(async () =>
            {
                var lowestMarketPrice = await GetLowestMarketPriceAsync(notSoldItem.AppId, notSoldItem.HashName);

                _logger.LogDebug($"listingId: '{notSoldItem.ListingId}' hashName: '{notSoldItem.HashName}' buyerPrice: '{notSoldItem.BuyerPrice}' lowestMarketPrice: '{lowestMarketPrice}'");
                if (notSoldItem.BuyerPrice > lowestMarketPrice)
                {
                    await _marketService.RemoveItemFromListingAsync(notSoldItem.ListingId);
                }
            }));
        }

        private async Task<List<MarketListing>> GetNotSoldItemsAsync()
        {
            var marketListings = await _marketService.GetAllMyListingsAsync();

            // filter items older than 3 days OR price is too hugh and it needed to be change now 
            var maximumDateForCheck = DateTime.UtcNow.AddDays(-DaysCountBeforeListingExpire);
            var result = marketListings
                .FindAll(x => x.SellDate < maximumDateForCheck || x.SellerPrice > PriceHelper.ExpensivePrice);
            _logger.LogDebug($"Total '{result.Count}' items on market older than {DaysCountBeforeListingExpire} days or price > 100 rubles");
            
            return result;
        }

        private async Task<uint?> GetLowestMarketPriceAsync(uint appId, string hashName)
        {
            var itemPrice = await _marketService.GetItemPriceAsync(appId, hashName);
            return itemPrice.LowestPrice;
        }
    }
}