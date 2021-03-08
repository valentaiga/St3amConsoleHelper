using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using SteamConsoleHelper.Abstractions.Enums;
using SteamConsoleHelper.Services.Messenger;
using SteamConsoleHelper.Storage;

namespace SteamConsoleHelper.Extensions
{
    public static class HostBuilderExtensions
    {
        public static IServiceCollection AddDataStoreService(this IServiceCollection services,
            IConfiguration configuration)
        {
            return Feature.DataStorage.GetConfiguration(configuration) 
                switch
            {
                "File" => services.AddSingleton<IDataStoreService, FileStoreService>(),
                "None" => services.AddSingleton<IDataStoreService, FakeDataStoreService>(),
                _ => throw new ArgumentOutOfRangeException($"{Feature.DataStorage}",
                    "DataStorage provider is not defined in configuration")
            };
        }

        public static IServiceCollection AddMessageProvider(this IServiceCollection services,
            IConfiguration configuration)
        {
            return Feature.Messenger.GetConfiguration(configuration) 
                switch
            {
                "Telegram" => services.AddSingleton<IMessageProvider, TelegramProvider>(),
                "Console" => services.AddSingleton<IMessageProvider, ConsoleProvider>(),
                _ => throw new ArgumentOutOfRangeException($"{Feature.Messenger}",
                    "Messenger is not defined in configuration")
            };
        }

        private static string GetConfiguration(this Feature feature, IConfiguration configuration)
            => configuration[$"Features:{feature}"];
    }
}