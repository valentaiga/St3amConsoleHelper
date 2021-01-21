using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using SteamConsoleHelper.BackgroundServices;
using SteamConsoleHelper.BackgroundServices.ScheduledJobs;
using SteamConsoleHelper.Common;
using SteamConsoleHelper.Resources;
using SteamConsoleHelper.Services;

namespace SteamConsoleHelper
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                CreateHostBuilder(args)
                    .Build()
                    .Run();
            }
            catch (Exception e)
            {
                // todo: use serilog instead CW
                Console.WriteLine(e);
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureHostConfiguration(hostBuilder =>
                {
                    // todo: fix config, builder cant find it
                    hostBuilder
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddEnvironmentVariables();
                })
                .ConfigureServices(services =>
                {
                    services
                        .AddSingleton<StoreService>()
                        .AddSingleton<LocalCacheService>()
                        .AddSingleton<ProfileSettings>()
                        .AddTransient<SteamUrlService>()
                        .AddTransient<WebRequestService>()
                        .AddSingleton<SteamUrlService>()
                        .AddSingleton<HttpClientFactory>()

                        .AddSingleton<DelayedExecutionPool>()

                        .AddTransient<InventoryService>()
                        .AddTransient<MarketService>()
                        .AddTransient<BoosterPackService>()

                        // job services
                        .AddSingleton<JobManager>()
                        .AddHostedService<DelayedExecutionService>()
                        .AddHostedService<HealthCheckService>()
                        // jobs
                        .AddHostedService<CheckMarketPriceJob>()
                        .AddHostedService<UnpackBoosterPacksJob>()
                        .AddHostedService<SellMarketableItemsJob>();
                });
    }
}
