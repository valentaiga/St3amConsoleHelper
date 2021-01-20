using System;
using System.Threading.Tasks;

using SteamConsoleHelper.Abstractions.Enums;
using SteamConsoleHelper.Exceptions;
using SteamConsoleHelper.Helpers;
using SteamConsoleHelper.Services;

namespace SteamConsoleHelper.BackgroundServices.ScheduledJobs
{
    public class UnpackBoosterPacksJob : ScheduledJobBase
    {
        private readonly InventoryService _inventoryService;
        private readonly BoosterPackService _boosterPackService;
        private readonly DelayedExecutionPool _delayedExecutionPool;

        public UnpackBoosterPacksJob(InventoryService inventoryService, BoosterPackService boosterPackService, DelayedExecutionPool delayedExecutionPool, JobManager jobManager)
            : base(jobManager)
        {
            _inventoryService = inventoryService;
            _boosterPackService = boosterPackService;
            _delayedExecutionPool = delayedExecutionPool;

            JobExecuteDelay = TimeSpan.FromHours(6);
        }

        public override async Task DoWorkAsync()
        {
            var inventoryItems = await _inventoryService.GetInventoryAsync();

            var packsToOpen = inventoryItems.FilterByType(ItemType.BoosterPack);

            Console.WriteLine($"{nameof(SellMarketableItemsJob)}: Filtered packs to open: '{packsToOpen.Count}'");

            foreach (var pack in packsToOpen)
            {
                _delayedExecutionPool.EnqueueRequestToPool(async () =>
                {
                    try
                    {
                        var cards = await _boosterPackService.UnpackBooster(pack.AppId, pack.AssetId);
                        cards.ForEach(x =>
                        {
                            if (x.IsFoil)
                            {
                                // todo: telegram bot integration to check sell page
                                Console.WriteLine($"{nameof(UnpackBoosterPacksJob)}: You got foil from '{pack.MarketName}' pack! Foil name - '{x.Name}'");
                            }
                        });
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