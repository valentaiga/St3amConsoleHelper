using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using SteamConsoleHelper.Abstractions.Enums;
using SteamConsoleHelper.Abstractions.Inventory;
using SteamConsoleHelper.Abstractions.Market;
using SteamConsoleHelper.Helpers;
using SteamConsoleHelper.Services;
using SteamConsoleHelper.Telegram;

namespace SteamConsoleHelper.BackgroundServices.ScheduledJobs
{
    /// <summary>
    /// Job for processing items in inventory
    /// </summary>
    public class InventoryItemsProcessJob : ScheduledJobBase<InventoryItemsProcessJob>
    {
        private readonly ILogger<InventoryItemsProcessJob> _logger;
        private readonly InventoryService _inventoryService;
        private readonly MarketService _marketService;
        private readonly BoosterPackService _boosterPackService;
        private readonly GemsService _gemsService;
        private readonly TelegramBotService _telegramBotService;
        private readonly DelayedExecutionPool _delayedExecutionPool;

        public InventoryItemsProcessJob(
            ILogger<InventoryItemsProcessJob> logger,
            InventoryService inventoryService,
            MarketService marketService,
            BoosterPackService boosterPackService,
            GemsService gemsService,
            TelegramBotService telegramBotService,
            DelayedExecutionPool delayedExecutionPool,
            JobManager jobManager)
            : base(logger, jobManager)
        {
            _logger = logger;
            _inventoryService = inventoryService;
            _marketService = marketService;
            _boosterPackService = boosterPackService;
            _gemsService = gemsService;
            _telegramBotService = telegramBotService;
            _delayedExecutionPool = delayedExecutionPool;

            JobExecuteDelay = TimeSpan.FromMinutes(10);
        }

        public override async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            var inventoryItems = await _inventoryService.GetInventoryAsync();
            var sentToMarketCards = await _marketService.GetAllMyListingsAsync();

            UnpackBoosterPacks(inventoryItems);
            SellTradableCards(inventoryItems, sentToMarketCards);
            OpenSacksOfGems(inventoryItems);
        }

        private void SellTradableCards(List<InventoryItem> inventoryItems, List<MarketListing> sentToMarketItems)
        {
            var cardsToSell = inventoryItems
                .FilterByMarketable()
                .FilterByCommodity()
                .FilterByType(ItemType.TradingCard)
                .FilterSentToMarketItems(sentToMarketItems);

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
                        await _telegramBotService.SendMessageAsync($"Sent to market foil: '{card.MarketName}' - '{calculatedPrice:##,00}' {Environment.NewLine}{card.IconUrl}");
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

        private void UnpackBoosterPacks(List<InventoryItem> inventoryItems)
        {
            var packsToOpen = inventoryItems.FilterByType(ItemType.BoosterPack);

            _logger.LogDebug($"Packs to open: '{packsToOpen.Count}'");

            Parallel.ForEach(packsToOpen, pack =>
                _delayedExecutionPool.EnqueueTaskToPool(async () =>
                {
                    _logger.LogInformation($"Opening booster pack '{pack.MarketHashName}' from '{pack.AppId}' game");
                    var cards = await _boosterPackService.UnpackBoosterAsync(pack.AppId, pack.AssetId);
                    cards.ForEach(async x =>
                    {
                        if (!x.IsFoil)
                        {
                            return;
                        }

                        await _telegramBotService.SendMessageAsync($"New foil from '{pack.MarketName}' - '{x.Name}'{Environment.NewLine}{x.ImageUrl}");
                        _logger.LogInformation($"Got foil from '{pack.MarketName}' pack! Foil name - '{x.Name}'");
                    });
                }));
        }
    }
}