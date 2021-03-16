using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using SteamConsoleHelper.Abstractions.Enums;
using SteamConsoleHelper.Abstractions.Inventory;
using SteamConsoleHelper.Abstractions.Market;
using SteamConsoleHelper.Common;
using SteamConsoleHelper.Helpers;
using SteamConsoleHelper.Services;
using SteamConsoleHelper.Services.Messenger;

namespace SteamConsoleHelper.BackgroundServices.ScheduledJobs
{
    /// <summary>
    /// Job for processing items in inventory
    /// </summary>
    public class InventoryItemsProcessJob : ScheduledJobBase<InventoryItemsProcessJob>
    {
        private const uint LogGemsAmount = 20_000;
        private const uint CardsAppId = 753;

        private int _grindItemsExecuteCounter = 6;

        private readonly ILogger<InventoryItemsProcessJob> _logger;
        private readonly SteamUrlService _steamUrlService;
        private readonly InventoryService _inventoryService;
        private readonly MarketService _marketService;
        private readonly BoosterPackService _boosterPackService;
        private readonly GemsService _gemsService;
        private readonly DelayedExecutionPool _delayedExecutionPool;
        private readonly IMessageProvider _messageProvider;

        public InventoryItemsProcessJob(
            ILogger<InventoryItemsProcessJob> logger,
            SteamUrlService steamUrlService,
            InventoryService inventoryService,
            MarketService marketService,
            BoosterPackService boosterPackService,
            GemsService gemsService,
            DelayedExecutionPool delayedExecutionPool,
            IMessageProvider messageProvider,
            JobManager jobManager)
            : base(logger, jobManager)
        {
            _logger = logger;
            _steamUrlService = steamUrlService;
            _inventoryService = inventoryService;
            _marketService = marketService;
            _boosterPackService = boosterPackService;
            _gemsService = gemsService;
            _delayedExecutionPool = delayedExecutionPool;
            _messageProvider = messageProvider;

            JobExecuteDelay = TimeSpan.FromMinutes(10);
        }

        public override async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            var inventoryItems = await _inventoryService.GetInventoryAsync();
            var sentToMarketCards = await _marketService.GetAllMyListingsAsync();

            UnpackBoosterPacks(inventoryItems);
            SellTradableCards(inventoryItems, sentToMarketCards, cancellationToken);
            await GrindItemsIntoGemsAsync(inventoryItems);
            OpenSacksOfGems(inventoryItems);
        }

        private void SellTradableCards(List<InventoryItem> inventoryItems, List<MarketListing> sentToMarketItems, CancellationToken cancellationToken)
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

                    var calculatedPrice = price.LowestPrice > PriceHelper.ExpensivePriceValue && price.LowestPrice < price.MedianPrice
                        ? PriceHelper.CalculateSellerPrice(price.LowestPrice, true)
                        : price.MedianPrice > price.LowestPrice
                            ? PriceHelper.CalculateSellerPrice(price.MedianPrice, true)
                            : PriceHelper.CalculateSellerPrice(price.LowestPrice, true);

                    if (card.IsCardFoil())
                    {
                        // bug: url is incorrect
                        var cardUrl = _steamUrlService.GetMarketItemListingUrl(CardsAppId, card.MarketHashName);
                        await _messageProvider.SendMessageAsync($"Sent to market foil: '{card.MarketName}' - '{PriceHelper.ConvertToRubles(calculatedPrice)}' {Environment.NewLine}{cardUrl}", cancellationToken);
                        _logger.LogInformation($"Sending foil '{card.MarketHashName}'. My price '{calculatedPrice}', lowest price '{price.LowestPrice}', median price '{price.MedianPrice}'");
                    }

                    await _marketService.SellItemAsync(card, calculatedPrice);
                }));
        }

        private void OpenSacksOfGems(List<InventoryItem> inventoryItems)
        {
            var gemsAmount = (uint)inventoryItems.FilterByType(ItemType.Gems).Sum(x => x.Amount);
            _logger.LogDebug($"Total {gemsAmount} gems unpacked");

            // don't open more sacks than needed
            if (gemsAmount > LogGemsAmount)
            {
                return;
            }

            var amountToOpen = (LogGemsAmount - gemsAmount) / 1000;

            inventoryItems
                .FilterByType(ItemType.SackOfGems)
                .ForEach(sack =>
                {
                    if (amountToOpen <= 0)
                    {
                        return;
                    }

                    amountToOpen -= sack.Amount;
                    _delayedExecutionPool.EnqueueTaskToPool(async () =>
                    {
                        _logger.LogDebug($"Opening sack of gems with {sack.Amount * 1000} gems");
                        await _gemsService.GrindSackIntoGems(sack, amountToOpen);
                    });
                });
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
                    cards.ForEach(x =>
                    {
                        if (!x.IsFoil)
                        {
                            return;
                        }
                        
                        _logger.LogInformation($"Got foil from '{pack.MarketName}' pack! Foil name - '{x.Name}'");
                    });
                }));
        }

        private async ValueTask GrindItemsIntoGemsAsync(List<InventoryItem> inventoryItems)
        {
            // actually I dont want to move this method to separated job but we call this method every 30 mins
            if (_grindItemsExecuteCounter++ < 3)
            {
                return;
            }
            
            var potentialSellItems = inventoryItems
                .FilterByTypes(ItemType.Emoticon, ItemType.ProfileBackground);

            var distinctItems = potentialSellItems
                .GroupBy(x => x.MarketHashName)
                .Select(x => x.First())
                .ToList();

            var hashGemsDic = new Dictionary<string, bool>();
            foreach (var item in distinctItems)
            {
                if (item.MarketItemType == null)
                {
                    hashGemsDic.Add(item.MarketHashName, false);
                    continue;
                }

                try
                {
                    var gemsFromExchange =
                        await _gemsService.GetGemsForItemExchangeAsync(item.MarketFeeApp, item.MarketItemType.Value);
                    if (gemsFromExchange != 100)
                    {
                        hashGemsDic.Add(item.MarketHashName, false);
                        continue;
                    }

                    var price = await _marketService.GetItemPriceAsync(item.AppId, item.MarketHashName);
                    if (price.LowestPrice == null || price.LowestPrice > 3_00)
                    {
                        hashGemsDic.Add(item.MarketHashName, false);
                        continue;
                    }

                    hashGemsDic.Add(item.MarketHashName, true);
                    _logger.LogDebug($"Item with marketHashname '{item.MarketHashName}' added to gems grind pool (price: '{PriceHelper.ConvertToRubles(price.LowestPrice.Value)}')");
                }
                catch (Exception ex)
                {
                    hashGemsDic.Add(item.MarketHashName, false);
                    _logger.LogError(ex, $"Unknown error, item marketHashname: '{item.MarketHashName}', instanceId: '{item.InstanceId}'");
                }
                finally
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }

            Parallel.ForEach(potentialSellItems, x =>
            {
                if (hashGemsDic[x.MarketHashName])
                {
                    _delayedExecutionPool.EnqueueTaskToPool(async () => await _gemsService.GrindItemIntoGems(x));
                }
            });

            _grindItemsExecuteCounter = 0;
        }
    }
}