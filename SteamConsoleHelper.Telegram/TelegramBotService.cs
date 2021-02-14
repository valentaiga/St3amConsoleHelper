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

        public TelegramBotService(ILogger<TelegramBotService> logger, IConfiguration configuration)
        {
            _logger = logger;

            try
            {
                var token = configuration["Telegram:ApiToken"];
                _telegramBotClient = new TelegramBotClient(token);
                _telegramBotClient.OnMessage += OnMessageHandler;
            }
            catch (Exception e)
            {
                logger.LogCritical($"Cant connect to telegram bot. Reason: {e.Message}");
                throw;
            }
        }

        private string LastMessage { get; set; }

        private int AuthorId { get; set; }

        private ChatId AuthorChatId { get; set; }

        public async Task SendMessageAsync(string text)
        {
            if (AuthorChatId != null)
            {
                await _telegramBotClient.SendTextMessageAsync(AuthorChatId, text);
            }
        }

        public async Task<string> ReadMessageAsync()
        {
            _telegramBotClient.StartReceiving();
            _logger.LogInformation($"Awaiting user's answer...");

            while (string.IsNullOrEmpty(LastMessage))
            {
                await Task.Delay(TimeSpan.FromSeconds(0.1));
            }

            _telegramBotClient.StopReceiving();
            var result = LastMessage;
            LastMessage = null;

            return result;
        }

        public async Task<string> SendMessageAndReadAnswerAsync(string text)
        {
            await SendMessageAsync(text);
            return await ReadMessageAsync();
        }

        private async void OnMessageHandler(object sender, MessageEventArgs args)
        {
            var message = args.Message;

            // todo: store and read authorId and chatId in/from storage in initialization method
            if (AuthorChatId == null)
            {
                AuthorId = message.From.Id;
                AuthorChatId = new ChatId(message.Chat.Id);
            }

            if (message.From.Id != AuthorId)
            {
                return;
            }

            if (message.Type != MessageType.Text)
            {
                return;
            }

            try
            {
                await _telegramBotClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                LastMessage = message.Text;
            }
            catch
            {
                // sometimes message sent earlier and already deleted
            }
        }
    }
}