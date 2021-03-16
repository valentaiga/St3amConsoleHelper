using System.Threading;
using System.Threading.Tasks;

using SteamConsoleHelper.Telegram;

namespace SteamConsoleHelper.Services.Messenger
{
    public class TelegramProvider : IMessageProvider
    {
        private readonly TelegramBotService _telegramBotService;

        public TelegramProvider(TelegramBotService telegramBotService)
        {
            _telegramBotService = telegramBotService;
        }

        public async Task SendMessageAsync(string message, CancellationToken stoppingToken)
        {
            await _telegramBotService.SendMessageAsync(message, stoppingToken);
        }

        public async Task<string> ReadMessageAsync(CancellationToken stoppingToken)
        {
            return await _telegramBotService.ReadMessageAsync(stoppingToken);
        }
    }
}