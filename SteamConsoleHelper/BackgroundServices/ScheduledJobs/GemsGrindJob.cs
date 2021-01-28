using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SteamConsoleHelper.Services;

namespace SteamConsoleHelper.BackgroundServices.ScheduledJobs
{
    public class GemsGrindJob : ScheduledJobBase<GemsGrindJob>
    {
        private readonly InventoryService _inventoryService;
        private readonly ILogger<GemsGrindJob> _logger;

        public GemsGrindJob(
            InventoryService inventoryService,
            ILogger<GemsGrindJob> logger, 
            JobManager jobManager) 
            : base(logger, jobManager)
        {
            _inventoryService = inventoryService;
            _logger = logger;

            JobExecuteDelay = TimeSpan.FromHours(1);
        }

        public override async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            // todo: unpack boosters and grind backgrounds 2020 into gems
            var inventoryItems = await _inventoryService.GetInventoryAsync();


            throw new System.NotImplementedException();
        }
    }
}