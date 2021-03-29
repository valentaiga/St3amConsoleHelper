using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace SteamConsoleHelper.BackgroundServices
{
    public class DelayedExecutionPool
    {
        private readonly ILogger<DelayedExecutionPool> _logger;
        private readonly ConcurrentQueue<Func<Task>> _actionsQueue;

        public DelayedExecutionPool(ILogger<DelayedExecutionPool> logger)
        {
            _logger = logger;
            _actionsQueue = new ConcurrentQueue<Func<Task>>();
        }

        public void EnqueueTaskToPool(Func<Task> action)
        {
            _actionsQueue.Enqueue(action);
        }

        public Func<Task> DequeueTaskFromPool()
        {
            var actionsCount = _actionsQueue.Count;
            if (actionsCount > 0)
            {
                _logger.LogDebug($"'{actionsCount}' more actions in queue");
            }

            var success = _actionsQueue.TryDequeue(out var action) && action != null;

            return success ? action : () => Task.CompletedTask;
        }
    }
}