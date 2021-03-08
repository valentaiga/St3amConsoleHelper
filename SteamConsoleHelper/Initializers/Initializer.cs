using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using SteamConsoleHelper.Resources;
using SteamConsoleHelper.Services;
using SteamConsoleHelper.Telegram;

namespace SteamConsoleHelper.Initializers
{
    public class Initializer : IDisposable
    {
        private IServiceCollection _services;

        public Initializer(IServiceCollection services)
        {
            _services = services;
        }

        public void Initialize()
        {
            var t = Task.Run(InitializeAsync);
            t.Wait();
        }

        private async Task InitializeAsync()
        {
            var services = _services.BuildServiceProvider();

            await InitializeTelegramAsync(services);
            await InitializeSteamAuthenticationAsync(services);

            await Settings.InitializeAsync();
        }

        private ValueTask InitializeSteamAuthenticationAsync(ServiceProvider serviceProvider)
            => serviceProvider.GetRequiredService<SteamAuthenticationService>().InitializeAsync();

        private Task InitializeTelegramAsync(ServiceProvider serviceProvider)
            => serviceProvider.GetRequiredService<TelegramBotService>().InitializeAsync();

        public void Dispose()
        {
            _services = null;
            GC.SuppressFinalize(this);
        }
    }
}