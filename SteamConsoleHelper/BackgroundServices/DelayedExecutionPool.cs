using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace SteamConsoleHelper.BackgroundServices
{
    public class DelayedExecutionPool
    {
        private static readonly TimeSpan DefaultOnFailureDelay = TimeSpan.FromSeconds(0.5);
        
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

            return success ? action : async () => await Task.Delay(DefaultOnFailureDelay);
        }
            
    }
}