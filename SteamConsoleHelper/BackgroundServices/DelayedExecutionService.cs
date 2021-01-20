using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

namespace SteamConsoleHelper.BackgroundServices
{
    public class DelayedExecutionService : BackgroundService
    {
        private static readonly TimeSpan ExecutionDelay = TimeSpan.FromSeconds(4);

        private readonly DelayedExecutionPool _requestPool;

        public DelayedExecutionService(DelayedExecutionPool requestPool)
        {
            _requestPool = requestPool;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var action = _requestPool.DequeueRequestFromPool();

                action?.Invoke();

                await Task.Delay(ExecutionDelay, stoppingToken);
            }
        }
    }
}