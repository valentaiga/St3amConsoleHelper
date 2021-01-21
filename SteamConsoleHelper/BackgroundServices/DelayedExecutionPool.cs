using System;
using System.Collections.Concurrent;

namespace SteamConsoleHelper.BackgroundServices
{
    public class DelayedExecutionPool
    {
        private readonly ConcurrentQueue<Action> _actionsQueue;

        public DelayedExecutionPool()
        {
            _actionsQueue = new ConcurrentQueue<Action>();
        }

        public void EnqueueRequestToPool(Action action)
        {
            _actionsQueue.Enqueue(action);
        }

        public Action DequeueRequestFromPool()
        {
            var success = _actionsQueue.TryDequeue(out var action) && action != null;
            
            Console.WriteLine($"{nameof(DelayedExecutionPool)}: '{_actionsQueue.Count}' more actions in queue");

            return success ? action : null;
        }
    }
}