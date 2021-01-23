using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SteamConsoleHelper.Abstractions.Enums;
using SteamConsoleHelper.Abstractions.Market;
using SteamConsoleHelper.Exceptions;
using SteamConsoleHelper.Helpers;
using SteamConsoleHelper.Services;

namespace SteamConsoleHelper.BackgroundServices.ScheduledJobs
{
    public class SellMarketableItemsJob : ScheduledJobBase<SellMarketableItemsJob>
    {
        private readonly ILogger<SellMarketableItemsJob> _logger;
        private readonly InventoryService _inventoryService;
        private readonly MarketService _marketService;
        private readonly DelayedExecutionPool _delayedExecutionPool;

        public SellMarketableItemsJob(
            ILogger<SellMarketableItemsJob> logger, 
            InventoryService inventoryService, 
            MarketService marketService, 
            DelayedExecutionPool delayedExecutionPool, 
            JobManager jobManager)
            : base(logger, jobManager)
        {
            _logger = logger;
            _inventoryService = inventoryService;
            _marketService = marketService;
            _delayedExecutionPool = delayedExecutionPool;

            JobExecuteDelay = TimeSpan.FromHours(1);
        }

        public override async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            var inventoryItems = await _inventoryService.GetInventoryAsync();

            var cardsToSell = inventoryItems
                .FilterByMarketable()
                .FilterByCommodity()
                .FilterByType(ItemType.TradingCard);

            _logger.LogDebug($"Filtered cards to sell: '{cardsToSell.Count}'");

            foreach (var card in cardsToSell)
            {
                _delayedExecutionPool.EnqueueTaskToPool(async () =>
                {
                    var price = await _marketService.GetItemPriceAsync(card);

                    if (price.LowestPrice == null && price.MedianPrice == null)
                    {
                        return;
                    }

                    var calculatedPrice = price.LowestPrice > PriceHelper.ExpensivePrice && price.LowestPrice < price.MedianPrice
                        ? PriceHelper.CalculateSellerPrice(price.LowestPrice, true) 
                        : PriceHelper.CalculateSellerPrice(price.MedianPrice, true);

                    if (card.IsCardFoil())
                    {
                        _logger.LogInformation($"Sending foil '{card.MarketHashName}'. Cost '{calculatedPrice}', lowest price '{price.LowestPrice}', median price '{price.MedianPrice}'");
                    }

                    await _marketService.SellItemAsync(card, calculatedPrice);
                });
            }
        }
    }
}