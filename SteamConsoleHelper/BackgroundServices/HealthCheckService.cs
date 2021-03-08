using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

using SteamConsoleHelper.Common;
using SteamConsoleHelper.Exceptions;
using SteamConsoleHelper.Resources;
using SteamConsoleHelper.Services;

namespace SteamConsoleHelper.BackgroundServices
{
    /// <summary>
    /// Service for user's credentials check
    /// </summary>
    public class HealthCheckService : BackgroundService
    {
        private static readonly TimeSpan CheckDelay = TimeSpan.FromMinutes(1);
        
        private readonly SteamAuthenticationService _steamAuthenticationService;
        private readonly SteamUrlService _steamUrlService;
        private readonly WebRequestService _requestService;

        public HealthCheckService(SteamAuthenticationService steamAuthenticationService, SteamUrlService steamUrlService, WebRequestService requestService)
        {
            _steamAuthenticationService = steamAuthenticationService;
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
                    if (ex.Error == InternalError.RequestBadRequest || ex.Error == InternalError.RequestUnauthorized || ex.Error == InternalError.UserIsNotAuthenticated)
                    {
                        Settings.SetIsAuthenticatedStatus(false);
                        await _steamAuthenticationService.InitializeLoginAsync();
                    }
                }
                await Task.Delay(CheckDelay, stoppingToken);
            }
        }
    }
}