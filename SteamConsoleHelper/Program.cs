using System;
using System.IO;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;

using SteamConsoleHelper.BackgroundServices;
using SteamConsoleHelper.BackgroundServices.ScheduledJobs;
using SteamConsoleHelper.Common;
using SteamConsoleHelper.Extensions;
using SteamConsoleHelper.Initializers;
using SteamConsoleHelper.Services;
using SteamConsoleHelper.Telegram;
using SteamConsoleHelper.Web;

namespace SteamConsoleHelper
{
    class Program
    {
        private const string DefaultListenPort = "8080";

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
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseKestrel(options =>
                        options.ListenAnyIP(int.Parse(Environment.GetEnvironmentVariable("PORT") ??
                                                      DefaultListenPort)));
                })
                .ConfigureServices(services =>
                {
                    services
                        .AddDataStoreService(Configuration)
                        .AddMessageProvider(Configuration)
                        .AddSingleton<SteamAuthenticationService>()
                        .AddSingleton<TelegramBotService>()
                        .AddSingleton<HttpClientFactory>()
                        .AddTransient<SteamUrlService>()
                        .AddTransient<WebRequestService>()

                        .AddSingleton<DelayedExecutionPool>()

                        .AddTransient<InventoryService>()
                        .AddTransient<MarketService>()
                        .AddTransient<BoosterPackService>()
                        .AddTransient<GemsService>()

                        .AddSingleton<JobManager>();

                    InitializeServices(services);

                    // jobs
                    services.AddHostedService<DelayedExecutionService>()
                        .AddHostedService<HealthCheckService>()
                        .AddHostedService<CheckMarketPriceJob>()
                        .AddHostedService<InventoryItemsProcessJob>();
                });

        private static void InitializeServices(IServiceCollection services)
        {
            using var initializer = new Initializer(services);
            initializer.Initialize();
        }
    }
}
