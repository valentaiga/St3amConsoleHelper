using System;
using System.IO;

using Microsoft.AspNetCore.Hosting;
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
                    // todo: use web to login via steam
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseKestrel(options =>
                        options.ListenAnyIP(int.Parse(Environment.GetEnvironmentVariable("PORT") ??
                                                      DefaultListenPort)));
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton<ProfileSettings>()
                        .AddSingleton<TelegramBotService>();
                    if (FakeService.SteamAuthenticationService.IsFakeEnabled(Configuration))
                    {
                        services.AddSingleton<ISteamAuthenticationService, FakeSteamAuthenticationService>();
                    }
                    else
                    {
                        services.AddSingleton<ISteamAuthenticationService, SteamAuthenticationService>();
                    }
                    
                    InitializeServices(services);

                    services
                        .AddSingleton<HttpClientFactory>()
                        .AddTransient<SteamUrlService>()
                        .AddTransient<WebRequestService>()

                        .AddSingleton<DelayedExecutionPool>()

                        .AddTransient<InventoryService>()
                        .AddTransient<MarketService>()
                        .AddTransient<BoosterPackService>()
                        .AddTransient<GemsService>()

                        // job services
                        .AddSingleton<JobManager>()
                        .AddHostedService<DelayedExecutionService>()
                        .AddHostedService<HealthCheckService>()
                        // jobs
                        .AddHostedService<CheckMarketPriceJob>()
                        .AddHostedService<InventoryItemsProcessJob>();
                });

        private static void InitializeServices(IServiceCollection services)
        {
            var provider = services.BuildServiceProvider();

            provider.GetRequiredService<ISteamAuthenticationService>()
                .InitiateLoginAsync().GetAwaiter().GetResult();

            provider.GetRequiredService<ProfileSettings>()
                .InitializeAsync().GetAwaiter().GetResult();
        }
    }
}
