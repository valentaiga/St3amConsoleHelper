﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SteamConsoleHelper.Abstractions.Enums;
using SteamConsoleHelper.Exceptions;
using SteamConsoleHelper.Helpers;
using SteamConsoleHelper.Services;

namespace SteamConsoleHelper.BackgroundServices.ScheduledJobs
{
    public class UnpackBoosterPacksJob : ScheduledJobBase
    {
        private readonly ILogger<UnpackBoosterPacksJob> _logger;
        private readonly InventoryService _inventoryService;
        private readonly BoosterPackService _boosterPackService;
        private readonly DelayedExecutionPool _delayedExecutionPool;

        public UnpackBoosterPacksJob(
            ILogger<UnpackBoosterPacksJob> logger, 
            InventoryService inventoryService, 
            BoosterPackService boosterPackService, 
            DelayedExecutionPool delayedExecutionPool, 
            JobManager jobManager)
            : base(jobManager)
        {
            _logger = logger;
            _inventoryService = inventoryService;
            _boosterPackService = boosterPackService;
            _delayedExecutionPool = delayedExecutionPool;

            JobExecuteDelay = TimeSpan.FromHours(1);
        }

        public override async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            var inventoryItems = await _inventoryService.GetInventoryAsync();

            var packsToOpen = inventoryItems.FilterByType(ItemType.BoosterPack);

            _logger.LogDebug($"Filtered packs to open: '{packsToOpen.Count}'");

            foreach (var pack in packsToOpen)
            {
                _delayedExecutionPool.EnqueueActionToPool(async () =>
                {
                    try
                    {
                        _logger.LogInformation($"Opening booster pack '{pack.MarketHashName}' from '{pack.AppId}' game");
                        var cards = await _boosterPackService.UnpackBooster(pack.AppId, pack.AssetId);
                        cards.ForEach(x =>
                        {
                            if (x.IsFoil)
                            {
                                // todo: telegram bot integration to check sell page
                                _logger.LogInformation($"Got foil from '{pack.MarketName}' pack! Foil name - '{x.Name}'");
                            }
                        });
                    }
                    catch (InternalException e)
                    {
                        _logger.LogError(e.Message);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e.ToString());
                    }
                });
            }
        }
    }
}