using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using SteamConsoleHelper.Exceptions;

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

            _actionsQueue.Enqueue(async () =>
            {
                await Task.Delay(100);
                throw new InternalException(InternalError.UnexpectedError);
            });
        }

        public void EnqueueTaskToPool(Func<Task> action)
        {
            _actionsQueue.Enqueue(action);
        }

        public Func<Task> DequeueTaskFromPool()
        {
            var success = _actionsQueue.TryDequeue(out var action) && action != null;

            var actionsCount = _actionsQueue.Count;
            if (actionsCount > 0)
            {
                _logger.LogInformation($"'{actionsCount}' more actions in queue");
            }

            return success ? action : () => Task.CompletedTask;
        }
    }
}