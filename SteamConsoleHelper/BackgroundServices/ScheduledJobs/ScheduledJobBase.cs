using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

namespace SteamConsoleHelper.BackgroundServices.ScheduledJobs
{
    public abstract class ScheduledJobBase : BackgroundService
    {
        protected TimeSpan JobExecuteDelay;

        private readonly TimeSpan _firstExecuteDelay;
        private bool _isExecutedEarlier;

        protected ScheduledJobBase(JobManager jobManager)
        {
            _firstExecuteDelay = jobManager.GetDelayBeforeFirstJobRun();
        }
        
        // todo: check my market slots with high price every 30 minutes
        public abstract Task DoWorkAsync();

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!_isExecutedEarlier)
                {
                    await Task.Delay(_firstExecuteDelay, cancellationToken);
                    _isExecutedEarlier = true;
                }

                await DoWorkAsync();
                await Task.Delay(JobExecuteDelay, cancellationToken);
            }
        }
    }
}