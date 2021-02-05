using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using SteamConsoleHelper.Abstractions.Enums;
using SteamConsoleHelper.Abstractions.Inventory;
using SteamConsoleHelper.Helpers;
using SteamConsoleHelper.Services;

namespace SteamConsoleHelper.BackgroundServices.ScheduledJobs
{
    public class InventoryItemsProcessJob : ScheduledJobBase<InventoryItemsProcessJob>
    {
        private readonly ILogger<InventoryItemsProcessJob> _logger;
        private readonly InventoryService _inventoryService;
        private readonly MarketService _marketService;
        private readonly GemsService _gemsService;
        private readonly DelayedExecutionPool _delayedExecutionPool;

        public InventoryItemsProcessJob(
            ILogger<InventoryItemsProcessJob> logger, 
            InventoryService inventoryService, 
            MarketService marketService,
            GemsService gemsService,
            DelayedExecutionPool delayedExecutionPool, 
            JobManager jobManager)
            : base(logger, jobManager)
        {
            _logger = logger;
            _inventoryService = inventoryService;
            _marketService = marketService;
            _gemsService = gemsService;
            _delayedExecutionPool = delayedExecutionPool;

            JobExecuteDelay = TimeSpan.FromHours(1);
        }

        public override async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            var inventoryItems = await _inventoryService.GetInventoryAsync();

            SellTradableCards(inventoryItems);
            OpenSacksOfGems(inventoryItems);
        }

        private void SellTradableCards(List<InventoryItem> inventoryItems)
        {
            var cardsToSell = inventoryItems
                .FilterByMarketable()
                .FilterByCommodity()
                .FilterByType(ItemType.TradingCard);

            _logger.LogDebug($"Filtered cards to sell: '{cardsToSell.Count}'");
            
            Parallel.ForEach(cardsToSell, card =>
                _delayedExecutionPool.EnqueueTaskToPool(async () =>
                {
                    var price = await _marketService.GetItemPriceAsync(card);

                    if (price.LowestPrice == null)
                    {
                        _logger.LogInformation($"Steam servers are busy, lowest price is not available for '{card.MarketHashName}'");
                        return;
                    }

                    var calculatedPrice = price.LowestPrice > PriceHelper.ExpensivePrice && price.LowestPrice < price.MedianPrice
                        ? PriceHelper.CalculateSellerPrice(price.LowestPrice, true)
                        : price.MedianPrice > price.LowestPrice
                            ? PriceHelper.CalculateSellerPrice(price.MedianPrice, true)
                            : PriceHelper.CalculateSellerPrice(price.LowestPrice, true);

                    if (card.IsCardFoil())
                    {
                        _logger.LogInformation($"Sending foil '{card.MarketHashName}'. Cost '{calculatedPrice}', lowest price '{price.LowestPrice}', median price '{price.MedianPrice}'");
                    }

                    await _marketService.SellItemAsync(card, calculatedPrice);
                }));
        }

        private void OpenSacksOfGems(List<InventoryItem> inventoryItems)
        {
            var sacks = inventoryItems
                .FilterByType(ItemType.SackOfGems);

            Parallel.ForEach(sacks, sack =>
                _delayedExecutionPool.EnqueueTaskToPool(async () =>
                {
                    _logger.LogDebug($"Opening sack of gems with {sack.Amount * 1000} gems");
                    await _gemsService.GrindSackIntoGems(sack);
                }));
        }
    }
}