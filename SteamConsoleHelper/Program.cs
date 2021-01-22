using System;
using System.IO;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;

using SteamConsoleHelper.Abstractions.Enums;
using SteamConsoleHelper.Abstractions.Fakes;
using SteamConsoleHelper.BackgroundServices;
using SteamConsoleHelper.BackgroundServices.ScheduledJobs;
using SteamConsoleHelper.Common;
using SteamConsoleHelper.Extensions;
using SteamConsoleHelper.Resources;
using SteamConsoleHelper.Services;
using SteamConsoleHelper.Services.Fakes;

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
                .CreateLogger();

            try
            {
                CreateHostBuilder(args)
                    .Build()
                    .Run();
            }
            catch (Exception e)
            {
                Log.Fatal($"Application failed. Exception: {e}");
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
                    if (FakeService.SteamAuthenticationService.IsFakeEnabled(Configuration))
                    {
                        services.AddSingleton<ISteamAuthenticationService, FakeSteamAuthenticationService>();
                    }
                    else
                    {
                        services.AddSingleton<ISteamAuthenticationService, SteamAuthenticationService>();
                    }

                    services.AddSingleton<ProfileSettings>();

                    // initialization
                    var provider = services.BuildServiceProvider();
                    if (FakeService.SteamAuthenticationService.IsFakeEnabled(Configuration))
                    {
                        var steamAuthService = provider.GetRequiredService<ISteamAuthenticationService>();
                        steamAuthService.Login(null, null);
                    }

                    provider.GetRequiredService<ProfileSettings>().InitializeAsync().GetAwaiter().GetResult();

                    services
                        .AddSingleton<HttpClientFactory>()
                        .AddSingleton<StoreService>()
                        .AddSingleton<LocalCacheService>()
                        //.AddSingleton<ProfileSettings>()
                        .AddTransient<SteamUrlService>()
                        .AddTransient<WebRequestService>()

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
