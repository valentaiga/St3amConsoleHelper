using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

using SteamConsoleHelper.Common;
using SteamConsoleHelper.Exceptions;

namespace SteamConsoleHelper.BackgroundServices
{
    /// <summary>
    /// Service for user's credentials check
    /// </summary>
    public class HealthCheckService : BackgroundService
    {
        private static readonly TimeSpan CheckDelay = TimeSpan.FromMinutes(5);
        
        private readonly SteamUrlService _steamUrlService;
        private readonly WebRequestService _requestService;

        public HealthCheckService(SteamUrlService steamUrlService, WebRequestService requestService)
        {
            _steamUrlService = steamUrlService;
            _requestService = requestService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var url = _steamUrlService.GetNotificationsCountUrl();
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _requestService.GetRequestAsync(url);
                }
                catch (InternalException ex)
                {
                    if (ex.Error == InternalError.RequestBadRequest)
                    {
                        Console.WriteLine($"{nameof(HealthCheckService)}: Token is invalid, please refresh it.");
                    }
                }
                await Task.Delay(CheckDelay, stoppingToken);
            }
        }
    }
}