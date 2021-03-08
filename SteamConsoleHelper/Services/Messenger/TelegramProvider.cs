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

        public async Task SendMessageAsync(string message)
        {
            await _telegramBotService.SendMessageAsync(message);
        }

        public async Task<string> ReadMessageAsync()
        {
            return await _telegramBotService.ReadMessageAsync();
        }
    }
}