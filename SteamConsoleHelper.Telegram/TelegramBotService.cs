using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SteamConsoleHelper.Telegram
{
    public class TelegramBotService
    {
        private readonly ILogger<TelegramBotService> _logger;
        private readonly TelegramBotClient _telegramBotClient;
        private readonly int _authorId;
        private readonly ChatId _authorChatId;

        private string _lastMessage;

        public TelegramBotService(ILogger<TelegramBotService> logger, IConfiguration configuration)
        {
            _logger = logger;

            var token = configuration["Telegram:ApiToken"];
            _authorId = int.Parse(configuration["Telegram:AuthorId"]);
            _authorChatId = new ChatId(long.Parse(configuration["Telegram:AuthorChatId"]));

            _telegramBotClient = new TelegramBotClient(token);
            _telegramBotClient.OnMessage += OnMessageHandler;
        }

        public async Task SendMessageAsync(string text)
        {
            await _telegramBotClient.SendTextMessageAsync(_authorChatId, text);
        }

        public async Task<string> SendMessageAndReadAnswerAsync(string text)
        {
            await _telegramBotClient.SendTextMessageAsync(_authorChatId, text);
            
            _telegramBotClient.StartReceiving();
            _logger.LogInformation($"Awaiting user's answer...");

            while (string.IsNullOrEmpty(_lastMessage))
            {
                await Task.Delay(TimeSpan.FromSeconds(0.1));
            }

            _telegramBotClient.StopReceiving();
            var result = _lastMessage;
            _lastMessage = null;

            return result;
        }

        private async void OnMessageHandler(object sender, MessageEventArgs args)
        {
            var message = args.Message;

            if (message.From.Id != _authorId)
            {
                return;
            }

            if (message.Type != MessageType.Text)
            {
                return;
            }

            _lastMessage = message.Text;
            await _telegramBotClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
        }
    }
}