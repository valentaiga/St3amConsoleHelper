using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

using SteamConsoleHelper.Exceptions;

namespace SteamConsoleHelper.BackgroundServices.ScheduledJobs
{
    public abstract class ScheduledJobBase<T> : BackgroundService
    {
        protected TimeSpan JobExecuteDelay;

        private readonly ILogger<T> _logger;
        private readonly TimeSpan _firstExecuteDelay;

        private bool _isExecutedEarlier;

        protected ScheduledJobBase(ILogger<T> logger, JobManager jobManager)
        {
            _logger = logger;
            _firstExecuteDelay = jobManager.GetDelayBeforeFirstJobRun();
        }
        
        public abstract Task DoWorkAsync(CancellationToken cancellationToken);

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!_isExecutedEarlier)
                {
                    await Task.Delay(_firstExecuteDelay, cancellationToken);
                    _isExecutedEarlier = true;
                }

                try
                {
                    await DoWorkAsync(cancellationToken);
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
                    await Task.Delay(JobExecuteDelay, cancellationToken);
                }
            }
        }
    }
}