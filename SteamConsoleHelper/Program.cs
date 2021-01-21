using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using SteamConsoleHelper.BackgroundServices;
using SteamConsoleHelper.BackgroundServices.ScheduledJobs;
using SteamConsoleHelper.Common;
using SteamConsoleHelper.Resources;
using SteamConsoleHelper.Services;

namespace SteamConsoleHelper
{
    class Program
    {
        private static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics", LogEventLevel.Information)
                .CreateLogger();

            try
            {
                CreateHostBuilder(args)
                    .Build()
                    .Run();
            }
            catch (Exception e)
            {
                Log.Fatal($"Application failed.", e);
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
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
