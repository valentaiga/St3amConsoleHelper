using System;
using System.Threading.Tasks;

using SteamConsoleHelper.Abstractions.Enums;
using SteamConsoleHelper.Abstractions.Market;
using SteamConsoleHelper.Exceptions;
using SteamConsoleHelper.Helpers;
using SteamConsoleHelper.Services;

namespace SteamConsoleHelper.BackgroundServices.ScheduledJobs
{
    public class SellMarketableItemsJob : ScheduledJobBase
    {

        private readonly InventoryService _inventoryService;
        private readonly MarketService _marketService;
        private readonly DelayedExecutionPool _delayedExecutionPool;

        public SellMarketableItemsJob(InventoryService inventoryService, MarketService marketService, DelayedExecutionPool delayedExecutionPool, JobManager jobManager)
            : base(jobManager)
        {
            _inventoryService = inventoryService;
            _marketService = marketService;
            _delayedExecutionPool = delayedExecutionPool;

            JobExecuteDelay = TimeSpan.FromHours(1);
        }

        public override async Task DoWorkAsync()
        {
            var inventoryItems = await _inventoryService.GetInventoryAsync();

            var cardsToSell = inventoryItems
                .FilterByMarketable()
                .FilterByCommodity()
                .FilterByType(ItemType.TradingCard);

            Console.WriteLine($"{nameof(SellMarketableItemsJob)}: Filtered cards to sell: '{cardsToSell.Count}'");

            foreach (var card in cardsToSell)
            {
                _delayedExecutionPool.EnqueueRequestToPool(async () =>
                {
                    try
                    {
                        var price = await _marketService.GetItemPriceAsync(card);

                        if (price.LowestPrice == null && price.MedianPrice == null)
                        {
                            return;
                        }

                        var calculatedPrice = PriceHelper.CalculateSellerPrice(price.LowestPrice, true);
                        
                        if (card.IsCardFoil())
                        {
                            Console.WriteLine($"Sending foil '{card.MarketHashName}'. Cost '{calculatedPrice}', lowest price '{price.LowestPrice}', median price '{price.MedianPrice}'");
                        }

                        await _marketService.SellItemAsync(card, calculatedPrice);
                    }
                    catch (InternalException e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                });
            }
        }
    }
}