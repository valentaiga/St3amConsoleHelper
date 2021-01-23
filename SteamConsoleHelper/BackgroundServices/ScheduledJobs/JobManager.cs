using System;

namespace SteamConsoleHelper.BackgroundServices.ScheduledJobs
{
    public class JobManager
    {
        private static readonly object Locker = new object();

        private TimeSpan _currentDelay;

        public JobManager()
        {
            _currentDelay = TimeSpan.FromSeconds(5);
        }

        public TimeSpan GetDelayBeforeFirstJobRun()
        {
            lock (Locker)
            {
                var result = _currentDelay;
                _currentDelay = _currentDelay.Add(TimeSpan.FromMinutes(2));
                return result;
            }
        }
    }
}