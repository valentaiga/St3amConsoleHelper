using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using SteamConsoleHelper.Exceptions;

namespace SteamConsoleHelper.BackgroundServices
{
    public class DelayedExecutionService : BackgroundService
    {
        private static readonly TimeSpan ExecutionDelay = TimeSpan.FromSeconds(5);

        private readonly ILogger<DelayedExecutionPool> _logger;
        private readonly DelayedExecutionPool _requestPool;

        public DelayedExecutionService(ILogger<DelayedExecutionPool> logger, DelayedExecutionPool requestPool)
        {
            _logger = logger;
            _requestPool = requestPool;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // initial launch delay
            await Task.Delay(ExecutionDelay, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var action = _requestPool.DequeueActionFromPool();
                    action?.Invoke();
                }
                catch (InternalException e)
                {
                    _logger.LogError(e.Message);
                }
                catch (Exception e)
                {
                    var errorText = e.ToString();
                    _logger.LogError(errorText);
                    throw new InternalException(InternalError.UnexpectedError, errorText);
                }
                finally
                {
                    await Task.Delay(ExecutionDelay, stoppingToken);
                }
            }
        }
    }
}