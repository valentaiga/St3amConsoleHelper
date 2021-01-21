using System;
using System.Collections.Concurrent;

using Microsoft.Extensions.Logging;

namespace SteamConsoleHelper.BackgroundServices
{
    public class DelayedExecutionPool
    {
        private readonly ILogger<DelayedExecutionPool> _logger;
        private readonly ConcurrentQueue<Action> _actionsQueue;

        public DelayedExecutionPool(ILogger<DelayedExecutionPool> logger)
        {
            _logger = logger;
            _actionsQueue = new ConcurrentQueue<Action>();
        }

        public void EnqueueActionToPool(Action action)
        {
            _actionsQueue.Enqueue(action);
        }

        public Action DequeueActionFromPool()
        {
            var success = _actionsQueue.TryDequeue(out var action) && action != null;

            var actionsCount = _actionsQueue.Count;
            if (actionsCount > 0)
            {
                _logger.LogInformation($"'{actionsCount}' more actions in queue");
            }

            return success ? action : null;
        }
    }
}